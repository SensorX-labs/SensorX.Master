using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SensorX.Master.Application.UseCases.Orders.Commands.CreateOrder;
using SensorX.Master.Application.UseCases.Orders.Queries.GetOrders;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;
using MediatR;

namespace SensorX.Master.WebApi.API;

public static class OrderApi
{
    public static IEndpointRouteBuilder MapOrderApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orders");

        group.WithOpenApi();

        group.MapGet("/", async ([FromServices] IMediator mediator)
            => Results.Ok(await mediator.Send(new GetOrdersQuery())))
            .WithName("GetOrders")
            .WithDescription("Get list of orders");

        group.MapPost("/", async ([FromServices] IMediator mediator, CreateOrderCommand command)
            => Results.Created($"/orders/{command.Code}", await mediator.Send(command)))
            .WithName("CreateOrder")
            .WithDescription("Create a new order");

        return app;
    }
}