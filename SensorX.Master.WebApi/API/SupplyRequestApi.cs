using MediatR;
using Microsoft.AspNetCore.Mvc;
using SensorX.Master.Application.UseCases.SupplyRequests.Commands.CreateSupplyRequest;
using SensorX.Master.Application.UseCases.SupplyRequests.Commands.ProcessSupplyRequest;
using SensorX.Master.Application.UseCases.SupplyRequests.Queries;

namespace SensorX.Master.WebApi.API;

public static class SupplyRequestApi
{
    public static IEndpointRouteBuilder MapSupplyRequestApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/supply-requests").WithTags("SupplyRequests");

        group.WithOpenApi();

        group.MapPost("/", async ([FromServices] IMediator mediator, [FromBody] CreateSupplyRequestCommand command) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/supply-requests/{result.Value}", result)
                : Results.BadRequest(result);
        })
        .WithName("CreateSupplyRequest")
        .WithDescription("Tạo mới yêu cầu cung ứng vật tư");

        group.MapPost("/process", async ([FromServices] IMediator mediator, [FromBody] ProcessSupplyRequestCommand command) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("ProcessSupplyRequest")
        .WithDescription("Bổ sung phương án mua ngoài hoặc duyệt hoàn tất yêu cầu cung ứng");

        group.MapGet("/", async ([FromServices] IMediator mediator, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            var result = await mediator.Send(new GetPageListSupplyRequestsQuery(page, pageSize));
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
        .WithName("GetPageListSupplyRequests")
        .WithDescription("Lấy danh sách yêu cầu cung ứng phân trang");

        group.MapGet("/{id:guid}", async ([FromServices] IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new GetSupplyRequestByIdQuery(id));
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.NotFound(result);
        })
        .WithName("GetSupplyRequestById")
        .WithDescription("Lấy thông tin chi tiết yêu cầu cung ứng theo ID");

        return app;
    }
}
