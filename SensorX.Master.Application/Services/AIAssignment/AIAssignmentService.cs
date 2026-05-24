using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.Models.DataServiceModels;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Domain.Common;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

using SensorX.Master.Application.Services.AIAssignment.Models;

namespace SensorX.Master.Application.Services.AIAssignment;

public class AIAssignmentService(
    IDataServiceClient _dataServiceClient,
    IQueryBuilder<SaleStaff> _staffBuilder,
    IQueryBuilder<StaffContextPerformance> _performanceBuilder,
    IQueryExecutor _queryExecutor,
    ILogger<AIAssignmentService> _logger
) : IAIAssignmentService
{
    public async Task<AIAllocationResult> FindBestStaffForRFQAsync(RFQ rfq, CancellationToken cancellationToken = default)
    {
        var result = new AIAllocationResult();

        if (rfq.Items == null || rfq.Items.Count == 0)
        {
            _logger.LogWarning("RFQ {Id} không có sản phẩm nào để phân bổ", rfq.Id.Value);
            return result;
        }

        // 1. Get Product Policies
        var productIds = rfq.Items.Select(x => x.ProductId.Value).Distinct().ToArray();
        var productPolicies = await _dataServiceClient.GetProductPricingAsync(productIds);

        if (productPolicies == null || productPolicies.Length == 0)
        {
            _logger.LogWarning("Không lấy được dữ liệu chính sách giá cho RFQ {Id}", rfq.Id.Value);
            return result;
        }

        // 2. Calculate Item Weights
        var (itemWeights, totalWeight) = CalculateItemWeights(rfq, productPolicies);

        // 3. Get Available Staffs
        var availableStaffs = await GetAvailableStaffsAsync(rfq, cancellationToken);
        if (availableStaffs.Count == 0)
        {
            _logger.LogWarning("Không còn SaleStaff nào hợp lệ cho RFQ {Id}, đánh dấu AllRejected.", rfq.Id.Value);
            rfq.MaskAsAllRejected();
            return result;
        }

        // 4. Get Staff Performances
        var categoryIds = itemWeights.Values.Select(x => x.CategoryId).Distinct().ToList();
        var performances = await GetStaffPerformancesAsync(availableStaffs, categoryIds, cancellationToken);

        // 5. Find Best Staff
        return FindBestStaff(rfq, availableStaffs, performances, itemWeights, totalWeight);
    }

    private static (Dictionary<Guid, (Guid CategoryId, double Weight)> Weights, double TotalWeight) CalculateItemWeights(RFQ rfq, ProductPricingPolicyData[] productPolicies)
    {
        var itemWeights = new Dictionary<Guid, (Guid CategoryId, double Weight)>();

        // Bước 1: Tính trọng số mặc định dựa trên Giá Sàn (FloorPrice * Quantity)

        foreach (var item in rfq.Items)
        {
            var policy = productPolicies.FirstOrDefault(p => p.ProductId == item.ProductId.Value);
            if (policy != null)
            {
                var weight = (double)policy.FloorPrice * item.Quantity.Value;
                itemWeights.Add(item.ProductId.Value, (policy.CategoryId, weight));
            }
        }

        // Bước 2: Tính tổng trọng số của giỏ hàng
        var totalWeight = itemWeights.Values.Sum(x => x.Weight);

        // Bước 3: Fallback an toàn. Nếu các mặt hàng bị mất giá sàn (dẫn đến tổng trọng số = 0), 
        // ta sẽ cào bằng trọng số (chia đều mỗi item = 1.0) để chia đều sự quan tâm của hệ thống cho các danh mục.
        if (totalWeight <= 0)
        {
            totalWeight = 0; // reset
            foreach (var item in rfq.Items)
            {
                var policy = productPolicies.FirstOrDefault(p => p.ProductId == item.ProductId.Value);
                if (policy != null)
                {
                    itemWeights[item.ProductId.Value] = (policy.CategoryId, 1.0);
                    totalWeight += 1.0;
                }
            }
        }

        // Bước 4: Chặn DivideByZeroException. Nếu thật sự không có sản phẩm nào hợp lệ, ép tổng về 1.0
        if (totalWeight <= 0) totalWeight = 1.0;

        return (itemWeights, totalWeight);
    }

    private async Task<List<SaleStaff>> GetAvailableStaffsAsync(RFQ rfq, CancellationToken cancellationToken)
    {
        var activeStaffsQuery = _staffBuilder.QueryAsNoTracking.Where(s => s.Status == StaffStatus.Active);
        var activeStaffs = await _queryExecutor.ToListAsync(activeStaffsQuery, cancellationToken);

        var rejectedStaffIds = rfq.RejectedByStaffIds.Select(r => r.Value).ToList();
        return activeStaffs.Where(s => !rejectedStaffIds.Contains(s.Id.Value)).ToList();
    }

    private async Task<List<StaffContextPerformance>> GetStaffPerformancesAsync(List<SaleStaff> availableStaffs, List<Guid> categoryIds, CancellationToken cancellationToken)
    {
        var staffIds = availableStaffs.Select(s => s.Id.Value).ToList();
        var performanceQuery = _performanceBuilder.QueryAsNoTracking
            .Where(p => staffIds.Contains(p.StaffId) && categoryIds.Contains(p.CategoryId));
        return await _queryExecutor.ToListAsync(performanceQuery, cancellationToken);
    }

    private AIAllocationResult FindBestStaff(
        RFQ rfq,
        List<SaleStaff> availableStaffs,
        List<StaffContextPerformance> performances,
        Dictionary<Guid, (Guid CategoryId, double Weight)> itemWeights,
        double totalWeight)
    {
        SaleStaff? bestStaff = null;
        double highestFinalScore = -double.MaxValue;
        var snapshotList = new List<AllocationSnapshot>();

        double k = 1.5; // Hệ số trừng phạt quá tải
        double idleWeight = 0.1; // Trọng số khuyến khích thời gian rảnh rỗi

        foreach (var staff in availableStaffs)
        {
            // Tính AggregatedSkillScore
            double aggregatedSkillScore = 0;
            foreach (var item in rfq.Items)
            {
                if (!itemWeights.TryGetValue(item.ProductId.Value, out var weightData)) continue;

                var perf = performances.FirstOrDefault(p => p.StaffId == staff.Id.Value && p.CategoryId == weightData.CategoryId);
                double expectedScore = 0;

                if (perf != null)
                {
                    expectedScore = perf.CalculateExpectedCategoryScore();
                }
                else
                {
                    // Nếu chưa có lịch sử, tính với Beta(1,1) -> probability = 0.5, avgMargin = 0 -> expected = 0.5
                    var dummyPerf = new StaffContextPerformance();
                    expectedScore = dummyPerf.CalculateExpectedCategoryScore();
                }

                aggregatedSkillScore += (expectedScore * weightData.Weight);
            }
            aggregatedSkillScore /= totalWeight;

            // Điểm chốt hạ
            double finalScore = staff.CalculateFinalAllocationScore(aggregatedSkillScore, k, idleWeight);

            double idleHours = 0;
            if (staff.LastAssignedAt.HasValue)
            {
                idleHours = (DateTimeOffset.UtcNow - staff.LastAssignedAt.Value).TotalHours;
            }
            if (idleHours > 48) idleHours = 48;

            // Ghi nhận snapshot
            snapshotList.Add(new AllocationSnapshot
            {
                StaffId = staff.Id.Value,
                StaffName = staff.Name,
                AggregatedSkillScore = Math.Round(aggregatedSkillScore, 4),
                CurrentWorkload = staff.CurrentWorkload,
                IdleScore = Math.Round(idleHours * idleWeight, 4),
                FinalScore = Math.Round(finalScore, 4)
            });

            if (finalScore > highestFinalScore)
            {
                highestFinalScore = finalScore;
                bestStaff = staff;
            }
        }

        var result = new AIAllocationResult
        {
            CandidatesSnapshot = snapshotList.OrderByDescending(x => x.FinalScore).ToList()
        };

        if (bestStaff != null)
        {
            _logger.LogInformation("Tìm thấy Best Staff {StaffId} cho RFQ {RfqId} với FinalScore={Score}", bestStaff.Id.Value, rfq.Id.Value, highestFinalScore);
            result.WinnerStaffId = new StaffId(bestStaff.Id.Value);
        }
        else
        {
            _logger.LogWarning("Không tìm thấy Staff phù hợp nào cho RFQ {RfqId}", rfq.Id.Value);
        }

        return result;
    }
}
