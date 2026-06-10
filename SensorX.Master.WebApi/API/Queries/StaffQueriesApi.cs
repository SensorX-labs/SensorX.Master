using MediatR;
using Microsoft.AspNetCore.Mvc;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.Queries.Staff.GetStaffAIPerformance;
using SensorX.Master.WebApi.Extensions;

namespace SensorX.Master.WebApi.API.Queries;

public static class StaffQueriesApi
{
    public static IEndpointRouteBuilder MapStaffQueriesApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("staff").WithTags("Staff Queries");

        api.MapGet("/{staffId:guid}/ai-performance", GetStaffAIPerformance)
            .WithOpenApi(op =>
            {
                op.Summary = "Lấy chỉ số năng lực AI của nhân viên theo từng danh mục sản phẩm";
                op.Description = """
                    Trả về toàn bộ thống kê năng lực AI của nhân viên bao gồm:
                    - SuccessCount, FailureCount, AverageMargin theo từng CategoryId
                    - CurrentWorkload, IdleHours, PenaltyWorkload, BoostIdle (real-time)
                    - Alpha/Beta params cho Beta distribution visualization
                    """;
                return op;
            });

        return app;
    }

    private static async Task<IResult> GetStaffAIPerformance(
        [FromRoute] Guid staffId,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        Result<GetStaffAIPerformanceResponse> result = await mediator.Send(
            new GetStaffAIPerformanceQuery(staffId), cancellationToken);
        return result.ToResult();
    }
}
