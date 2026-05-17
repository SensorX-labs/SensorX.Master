using Microsoft.AspNetCore.Mvc;
using SensorX.Master.Application.Commands.Orders.CreateOrder;
using SensorX.Master.Application.Queries.Orders.GetDetailOrderById;
using SensorX.Master.Application.Queries.Orders.GetPageListOrder;
using SensorX.Master.WebApi.Extensions;
using MediatR;

namespace SensorX.Master.WebApi.API;

public static class OrderApi
{
    public static IEndpointRouteBuilder MapOrderApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders")
            .WithTags("Orders")
            .WithOpenApi();

        group.MapGet("/", GetPageListOrder)
            .WithName("GetPageListOrders")
            .WithDescription("Get paged list of orders");

        group.MapGet("/{orderId:guid}", GetDetailOrderById)
            .WithName("GetOrderDetail")
            .WithDescription("Get order detail by ID");

        group.MapPost("/", async ([FromServices] IMediator mediator, CreateOrderCommand command) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? Results.Created($"/orders/{result.Value}", result)
                : Results.BadRequest(result);
        })
            .WithName("CreateOrder")
            .WithDescription("Create a new order");

        return app;
    }

    private static async Task<IResult> GetPageListOrder(
        [AsParameters] GetPageListOrderQuery query,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(query);
        return result.ToResult();
    }

    private static async Task<IResult> GetDetailOrderById(
        [FromRoute] Guid orderId,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new GetDetailOrderByIdQuery(orderId));
        return result.ToResult();
    }
}
