using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.Models.DataServiceModels;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Domain.Common;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Services;

public class AIAssignmentService(
    IDataServiceClient _dataServiceClient,
    IQueryBuilder<SaleStaff> _staffBuilder,
    IQueryBuilder<StaffContextPerformance> _performanceBuilder,
    IQueryExecutor _queryExecutor,
    ILogger<AIAssignmentService> _logger
) : IAIAssignmentService
{
    public async Task<StaffId?> FindBestStaffForRFQAsync(RFQ rfq, CancellationToken cancellationToken = default)
    {
        if (rfq.Items == null || rfq.Items.Count == 0)
        {
            _logger.LogWarning("RFQ {Id} không có sản phẩm nào để phân bổ", rfq.Id.Value);
            return null;
        }

        // 1. Get Product Policies
        var productIds = rfq.Items.Select(x => x.ProductId.Value).Distinct().ToArray();
        var productPolicies = await _dataServiceClient.GetProductPricingAsync(productIds);

        if (productPolicies == null || productPolicies.Length == 0)
        {
            _logger.LogWarning("Không lấy được dữ liệu chính sách giá cho RFQ {Id}", rfq.Id.Value);
            return null;
        }

        // 2. Calculate Item Weights
        var (itemWeights, totalWeight) = CalculateItemWeights(rfq, productPolicies);

        // 3. Get Available Staffs
        var availableStaffs = await GetAvailableStaffsAsync(rfq, cancellationToken);
        if (availableStaffs.Count == 0)
        {
            _logger.LogWarning("Không còn SaleStaff nào hợp lệ cho RFQ {Id}, đánh dấu AllRejected.", rfq.Id.Value);
            rfq.MaskAsAllRejected();
            return null;
        }

        // 4. Get Staff Performances
        var categoryIds = itemWeights.Values.Select(x => x.CategoryId).Distinct().ToList();
        var performances = await GetStaffPerformancesAsync(availableStaffs, categoryIds, cancellationToken);

        // 5. Find Best Staff
        return FindBestStaff(rfq, availableStaffs, performances, itemWeights, totalWeight);
    }

    private static (Dictionary<Guid, (Guid CategoryId, double Weight)> Weights, double TotalWeight) CalculateItemWeights(RFQ rfq, ProductPricingPolicyData[] productPolicies)
    {
        var itemWeights = ExtractBaseItemWeights(rfq, productPolicies);
        var totalWeight = itemWeights.Values.Sum(x => x.Weight);

        if (totalWeight <= 0)
        {
            return ApplyFallbackWeights(rfq, productPolicies, itemWeights);
        }

        return (itemWeights, totalWeight);
    }

    private static Dictionary<Guid, (Guid CategoryId, double Weight)> ExtractBaseItemWeights(RFQ rfq, ProductPricingPolicyData[] productPolicies)
    {
        var itemWeights = new Dictionary<Guid, (Guid CategoryId, double Weight)>();
        foreach (var item in rfq.Items)
        {
            var policy = productPolicies.FirstOrDefault(p => p.ProductId == item.ProductId.Value);
            if (policy != null)
            {
                var weight = (double)policy.FloorPrice * item.Quantity.Value;
                itemWeights.Add(item.ProductId.Value, (policy.CategoryId, weight));
            }
        }
        return itemWeights;
    }

    private static (Dictionary<Guid, (Guid CategoryId, double Weight)>, double) ApplyFallbackWeights(RFQ rfq, ProductPricingPolicyData[] productPolicies, Dictionary<Guid, (Guid CategoryId, double Weight)> itemWeights)
    {
        foreach (var item in rfq.Items)
        {
            var policy = productPolicies.FirstOrDefault(p => p.ProductId == item.ProductId.Value);
            if (policy != null)
            {
                itemWeights[item.ProductId.Value] = (policy.CategoryId, 1.0);
            }
        }
        return (itemWeights, itemWeights.Count);
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

    private StaffId? FindBestStaff(
        RFQ rfq,
        List<SaleStaff> availableStaffs,
        List<StaffContextPerformance> performances,
        Dictionary<Guid, (Guid CategoryId, double Weight)> itemWeights,
        double totalWeight)
    {
        SaleStaff? bestStaff = null;
        double highestFinalScore = -double.MaxValue;

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

            if (finalScore > highestFinalScore)
            {
                highestFinalScore = finalScore;
                bestStaff = staff;
            }
        }

        if (bestStaff != null)
        {
            _logger.LogInformation("Tìm thấy Best Staff {StaffId} cho RFQ {RfqId} với FinalScore={Score}", bestStaff.Id.Value, rfq.Id.Value, highestFinalScore);
            return new StaffId(bestStaff.Id.Value);
        }

        _logger.LogWarning("Không tìm thấy Staff phù hợp nào cho RFQ {RfqId}", rfq.Id.Value);
        return null;
    }
}
