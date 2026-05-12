using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SensorX.Master.Application.UseCases.TransferOrders.Commands.CreateTransferOrder;
using SensorX.Master.Domain.StrongIDs;
using MediatR;

namespace SensorX.Master.WebApi.API;

public static class TransferOrderApi
{
    public static IEndpointRouteBuilder MapTransferOrderApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/transfer-orders");

        group.WithOpenApi();

        group.MapPost("/", async ([FromServices] IMediator mediator, CreateTransferOrderCommand command)
            => Results.Created($"/transfer-orders/{command.Code}", await mediator.Send(command)))
            .WithName("CreateTransferOrder")
            .WithDescription("Create a new transfer order");

        return app;
    }
}