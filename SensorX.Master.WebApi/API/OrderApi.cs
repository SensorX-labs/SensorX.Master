using Microsoft.AspNetCore.Mvc;
using SensorX.Master.Application.Commands.Orders.CancelOrder;
using SensorX.Master.Application.Commands.Orders.CreateOrder;
using SensorX.Master.Application.Queries.Orders.GetDetailOrderById;
using SensorX.Master.Application.Queries.Orders.GetMyOrderById;
using SensorX.Master.Application.Queries.Orders.GetMyOrders;
using SensorX.Master.Application.Queries.Orders.GetOrderStats;
using SensorX.Master.Application.Queries.Orders.GetPageListOrder;
using SensorX.Master.Application.Queries.Orders.GetOrderPaymentStatus;
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

        group.MapGet("/stats", GetOrderStats)
            .WithName("GetOrderStats")
            .WithDescription("Get order statistics");

        group.MapGet("/my", GetMyOrders)
            .WithName("GetMyOrders")
            .WithDescription("Get paged list of current customer's orders");

        group.MapGet("/{orderId:guid}", GetDetailOrderById)
            .WithName("GetOrderDetail")
            .WithDescription("Get order detail by ID");

        group.MapGet("/{orderId:guid}/payment-status", GetOrderPaymentStatus)
            .WithName("GetOrderPaymentStatus")
            .WithDescription("Check payment status of an order");

        group.MapGet("/my/{orderId:guid}", GetMyOrderById)
            .WithName("GetMyOrderDetail")
            .WithDescription("Get current customer's order detail by ID");

        group.MapPost("/", async ([FromServices] IMediator mediator, CreateOrderCommand command) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? Results.Created($"/orders/{result.Value}", result)
                : Results.BadRequest(result);
        })
            .WithName("CreateOrder")
            .WithDescription("Create a new order");

        group.MapPost("/my/{orderId:guid}/cancel", CancelMyOrder)
            .WithName("CancelMyOrder")
            .WithDescription("Cancel a pending-payment order (authenticated customer only)");

        return app;
    }

    private static async Task<IResult> GetPageListOrder(
        [AsParameters] GetPageListOrderQuery query,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(query);
        return result.ToResult();
    }

    private static async Task<IResult> GetOrderStats(
        [AsParameters] GetOrderStatsQuery query,
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

    private static async Task<IResult> GetOrderPaymentStatus(
        [FromRoute] Guid orderId,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new GetOrderPaymentStatusQuery(orderId));
        return result.ToResult();
    }

    private static async Task<IResult> GetMyOrders(
        [AsParameters] GetMyOrdersQuery query,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(query);
        return result.ToResult();
    }

    private static async Task<IResult> GetMyOrderById(
        [FromRoute] Guid orderId,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new GetMyOrderByIdQuery(orderId));
        return result.ToResult();
    }

    private static async Task<IResult> CancelMyOrder(
        [FromRoute] Guid orderId,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new CancelOrderCommand(orderId));
        return result.ToResult();
    }
}
