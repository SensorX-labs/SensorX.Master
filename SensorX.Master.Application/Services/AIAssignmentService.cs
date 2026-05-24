using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.Interfaces;
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
    IRepository<SaleStaff> _staffRepository,
    IQueryExecutor _queryExecutor,
    ILogger<AIAssignmentService> _logger
) : IAIAssignmentService
{
    public async Task AssignStaffToRFQAsync(RFQ rfq, CancellationToken cancellationToken = default)
    {
        if (rfq.Items == null || rfq.Items.Count == 0)
        {
            _logger.LogWarning("RFQ {Id} không có sản phẩm nào để phân bổ", rfq.Id.Value);
            return;
        }

        var productIds = rfq.Items.Select(x => x.ProductId.Value).Distinct().ToArray();
        var productPolicies = await _dataServiceClient.GetProductPricingAsync(productIds);

        if (productPolicies == null || productPolicies.Length == 0)
        {
            _logger.LogWarning("Không lấy được dữ liệu chính sách giá cho RFQ {Id}", rfq.Id.Value);
            return;
        }

        // Tính trọng số của từng sản phẩm trong RFQ (W_j = Quantity_j * BasePrice_j)
        // Lưu ý: BasePrice có thể là FloorPrice
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

        var totalWeight = itemWeights.Values.Sum(x => x.Weight);
        if (totalWeight <= 0)
        {
            // Fallback nếu weight = 0, chia đều weight
            totalWeight = 1;
            foreach (var item in rfq.Items)
            {
                var policy = productPolicies.FirstOrDefault(p => p.ProductId == item.ProductId.Value);
                if (policy != null)
                {
                    itemWeights[item.ProductId.Value] = (policy.CategoryId, 1.0);
                }
            }
            totalWeight = itemWeights.Count;
        }

        // Lấy danh sách nhân viên Sale đang Active
        var activeStaffsQuery = _staffBuilder.QueryAsNoTracking.Where(s => s.Status == StaffStatus.Active);
        var activeStaffs = await _queryExecutor.ToListAsync(activeStaffsQuery, cancellationToken);

        // Loại bỏ những nhân viên đã từ chối

        var rejectedStaffIds = rfq.RejectedByStaffIds.Select(r => r.Value).ToList();
        var availableStaffs = activeStaffs.Where(s => !rejectedStaffIds.Contains(s.Id.Value)).ToList();

        if (availableStaffs.Count == 0)
        {
            _logger.LogWarning("Không còn SaleStaff nào hợp lệ cho RFQ {Id}, đánh dấu AllRejected.", rfq.Id.Value);
            rfq.MaskAsAllRejected();
            return;
        }

        // Lấy Performance của các nhân viên này
        var categoryIds = itemWeights.Values.Select(x => x.CategoryId).Distinct().ToList();
        var staffIds = availableStaffs.Select(s => s.Id.Value).ToList();

        var performanceQuery = _performanceBuilder.QueryAsNoTracking
            .Where(p => staffIds.Contains(p.StaffId) && categoryIds.Contains(p.CategoryId));


        var performances = await _queryExecutor.ToListAsync(performanceQuery, cancellationToken);

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
            _logger.LogInformation("Phân bổ RFQ {RfqId} cho Staff {StaffId} với FinalScore={Score}", rfq.Id.Value, bestStaff.Id.Value, highestFinalScore);
            rfq.Assign(new StaffId(bestStaff.Id.Value));

            // Cập nhật SaleStaff state

            var dbStaff = await _staffRepository.GetByIdAsync(new StaffId(bestStaff.Id.Value), cancellationToken);
            if (dbStaff != null)
            {
                dbStaff.AssignRfq();
                await _staffRepository.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
