using MediatR;
using Microsoft.AspNetCore.Mvc;
using SensorX.Master.Application.Commands.Warehouses;
using SensorX.Master.Application.DTOs;
using SensorX.Master.Application.Queries.Warehouses;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;

namespace SensorX.Master.WebApi.API;

public static class WarehouseApi
{
    public static IEndpointRouteBuilder MapWarehouseApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("warehouses").WithTags("Warehouses");

        api.MapPost("", CreateWarehouse)
            .WithOpenApi(op => { op.Summary = "Create a new warehouse"; return op; });

        api.MapGet("{id:guid}", GetWarehouseById)
            .WithOpenApi(op => { op.Summary = "Get warehouse by ID"; return op; });

        api.MapGet("", GetAllWarehouses)
            .WithOpenApi(op => { op.Summary = "Get all warehouses"; return op; });

        api.MapDelete("{id:guid}", DeleteWarehouse)
            .WithOpenApi(op => { op.Summary = "Delete a warehouse"; return op; });

        api.MapPost("{id:guid}/deactivate", DeactivateWarehouse)
            .WithOpenApi(op => { op.Summary = "Deactivate a warehouse"; return op; });

        api.MapPost("{id:guid}/activate", ActivateWarehouse)
            .WithOpenApi(op => { op.Summary = "Activate a warehouse"; return op; });

        return app;
    }

    private static async Task<IResult> CreateWarehouse(
        [FromBody] CreateWarehouseCommand command,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }

    private static async Task<IResult> GetWarehouseById(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var query = new GetWarehouseByIdQuery(id);
        var result = await mediator.Send(query);
        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.NotFound(result);
    }

    private static async Task<IResult> GetAllWarehouses(
        [FromServices] IMediator mediator)
    {
        var query = new GetAllWarehousesQuery();
        var result = await mediator.Send(query);
        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }

    private static async Task<IResult> DeleteWarehouse(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var command = new DeleteWarehouseCommand(id);
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }

    private static async Task<IResult> DeactivateWarehouse(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var command = new DeactivateWarehouseCommand(id);
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }

    private static async Task<IResult> ActivateWarehouse(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator)
    {
        var command = new ActivateWarehouseCommand(id);
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}