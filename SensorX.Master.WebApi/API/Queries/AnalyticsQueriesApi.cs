using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SensorX.Master.Application.Queries.Analytics.GetDashboardTransactionStats;
using SensorX.Master.Application.Queries.Analytics.GetRevenueReport;
using SensorX.Master.Application.Queries.Analytics.GetBusinessReportStats;
using SensorX.Master.WebApi.Extensions;
using System.Threading.Tasks;

namespace SensorX.Master.WebApi.API.Queries;

public static class AnalyticsQueriesApi
{
    public static IEndpointRouteBuilder MapAnalyticsQueriesApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("analytics").WithTags("Analytics Queries");

        api.MapGet("dashboard", GetDashboardTransactionStats).WithOpenApi(operation =>
        {
            operation.Summary = "Lấy thống kê Bảng điều khiển (Master Transaction)";
            return operation;
        });

        api.MapGet("revenue", GetRevenueReport).WithOpenApi(operation =>
        {
            operation.Summary = "Lấy báo cáo Doanh thu tài chính";
            return operation;
        });

        api.MapGet("business-report", GetBusinessReportStats).WithOpenApi(operation =>
        {
            operation.Summary = "Lấy thống kê Báo cáo Kinh doanh";
            return operation;
        });

        return api;
    }

    private static async Task<IResult> GetDashboardTransactionStats(
        [AsParameters] GetDashboardTransactionStatsQuery query,
        [FromServices] IMediator mediator
    )
    {
        var result = await mediator.Send(query);
        return result.ToResult();
    }

    private static async Task<IResult> GetRevenueReport(
        [AsParameters] GetRevenueReportQuery query,
        [FromServices] IMediator mediator
    )
    {
        var result = await mediator.Send(query);
        return result.ToResult();
    }

    private static async Task<IResult> GetBusinessReportStats(
        [AsParameters] GetBusinessReportStatsQuery query,
        [FromServices] IMediator mediator
    )
    {
        var result = await mediator.Send(query);
        return result.ToResult();
    }
}
