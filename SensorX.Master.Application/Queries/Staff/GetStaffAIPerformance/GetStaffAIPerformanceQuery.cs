using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Staff.GetStaffAIPerformance;

public sealed record GetStaffAIPerformanceQuery(Guid StaffId)
    : IRequest<Result<GetStaffAIPerformanceResponse>>;

public sealed record StaffCategoryPerformanceDto(
    Guid CategoryId,
    string CategoryName,
    int SuccessCount,
    int FailureCount,
    double TotalMarginAccumulated,
    double AverageMargin,
    double WinRate,
    double AlphaParam,  // SuccessCount + 1
    double BetaParam    // FailureCount + 1
);

public sealed record GetStaffAIPerformanceResponse(
    Guid StaffId,
    string StaffName,
    int CurrentWorkload,
    DateTimeOffset? LastAssignedAt,
    double IdleHours,
    double PenaltyWorkload,
    double BoostIdle,
    IReadOnlyList<StaffCategoryPerformanceDto> CategoryPerformances
);
