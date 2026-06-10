using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Staff.GetStaffAIPerformance;

public sealed class GetStaffAIPerformanceHandler(
    IQueryBuilder<SaleStaff> staffBuilder,
    IQueryBuilder<StaffContextPerformance> performanceBuilder,
    IQueryExecutor queryExecutor
) : IRequestHandler<GetStaffAIPerformanceQuery, Result<GetStaffAIPerformanceResponse>>
{
    public async Task<Result<GetStaffAIPerformanceResponse>> Handle(
        GetStaffAIPerformanceQuery request,
        CancellationToken cancellationToken)
    {
        var staffQuery = staffBuilder.QueryAsNoTracking
            .Where(s => (Guid)s.Id == request.StaffId);

        var staff = await queryExecutor.FirstOrDefaultAsync(staffQuery, cancellationToken);

        if (staff == null)
            return Result<GetStaffAIPerformanceResponse>.Failure("Không tìm thấy nhân viên.");

        var performances = await queryExecutor.ToListAsync(
            performanceBuilder.QueryAsNoTracking
                .Where(p => p.StaffId == request.StaffId),
            cancellationToken);

        // Tính IdleHours dựa trên CurrentWorkload và LastAssignedAt
        double idleHours = 0;
        if (staff.CurrentWorkload == 0 && staff.LastAssignedAt.HasValue)
        {
            idleHours = (DateTimeOffset.UtcNow - staff.LastAssignedAt.Value).TotalHours;
        }

        // Tính Penalty và Boost với hyperparams mặc định k=1.5, idleWeight=0.1
        // (Frontend có thể fetch hyperparams riêng để hiển thị chính xác hơn)
        const double k = 1.5;
        const double idleWeight = 0.1;
        double penaltyWorkload = 1.0 / Math.Pow(staff.CurrentWorkload + 1, k);
        double boostIdle = Math.Tanh(idleHours / 24.0) * idleWeight;

        var categoryDtos = performances.Select(p =>
        {
            double avgMargin = p.SuccessCount > 0
                ? p.TotalMarginAccumulated / p.SuccessCount
                : 0.0;

            int total = p.SuccessCount + p.FailureCount;
            double winRate = total > 0 ? (double)p.SuccessCount / total : 0.0;

            return new StaffCategoryPerformanceDto(
                p.CategoryId,
                string.Empty,           // Tên danh mục: frontend tự resolve từ Data service
                p.SuccessCount,
                p.FailureCount,
                p.TotalMarginAccumulated,
                avgMargin,
                winRate,
                p.SuccessCount + 1.0,   // Alpha param cho Beta dist
                p.FailureCount + 1.0    // Beta param cho Beta dist
            );
        }).ToList();

        var response = new GetStaffAIPerformanceResponse(
            request.StaffId,
            staff.Name,
            staff.CurrentWorkload,
            staff.LastAssignedAt,
            idleHours,
            penaltyWorkload,
            boostIdle,
            categoryDtos
        );

        return Result<GetStaffAIPerformanceResponse>.Success(response);
    }
}
