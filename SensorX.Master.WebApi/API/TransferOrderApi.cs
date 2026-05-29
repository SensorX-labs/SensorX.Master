using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SensorX.Master.Application.UseCases.TransferOrders.Commands.CreateTransferOrder;
using SensorX.Master.Application.UseCases.TransferOrders.Queries;
using SensorX.Master.Domain.StrongIDs;
using MediatR;

namespace SensorX.Master.WebApi.API;

public static class TransferOrderApi
{
    public static IEndpointRouteBuilder MapTransferOrderApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/transfer-orders").WithTags("TransferOrders");

        group.WithOpenApi();

        group.MapPost("/", async ([FromServices] IMediator mediator, CreateTransferOrderCommand command) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? Results.Created($"/transfer-orders/{command.Code}", result)
                : Results.BadRequest(result);
        })
            .WithName("CreateTransferOrder")
            .WithDescription("Create a new transfer order");

        group.MapGet("/", async ([FromServices] IMediator mediator, [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        {
            var result = await mediator.Send(new GetPageListTransferOrdersQuery(page, pageSize));
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.BadRequest(result);
        })
            .WithName("GetPageListTransferOrders")
            .WithDescription("Lấy danh sách lệnh điều chuyển phân trang");

        group.MapGet("/{id:guid}", async ([FromServices] IMediator mediator, Guid id) =>
        {
            var result = await mediator.Send(new GetTransferOrderByIdQuery(id));
            return result.IsSuccess
                ? Results.Ok(result)
                : Results.NotFound(result);
        })
            .WithName("GetTransferOrderById")
            .WithDescription("Lấy thông tin chi tiết lệnh điều chuyển theo ID");

        return app;
    }
}