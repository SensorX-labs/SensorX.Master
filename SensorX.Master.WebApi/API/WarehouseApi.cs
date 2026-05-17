using MediatR;
using Microsoft.AspNetCore.Mvc;
using SensorX.Master.Application.Commands.Warehouses;
using SensorX.Master.Application.DTOs;
using SensorX.Master.Application.Queries.Warehouses;
using SensorX.Master.Application.Services;

namespace SensorX.Master.WebApi.API;

public static class WarehouseApi
{
    public static IEndpointRouteBuilder MapWarehouseApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("warehouses").WithTags("Warehouses");

        api.MapGet("inventory/total", GetTotalInventory)
            .WithOpenApi(op => { op.Summary = "Get total consolidated inventory from all warehouses"; return op; });

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

    private static async Task<IResult> GetTotalInventory(
        [FromServices] IWarehouseQueryService warehouseQueryService,
        CancellationToken cancellationToken)
    {
        var flatItems = await warehouseQueryService.GetTotalInventoryRowsAsync(cancellationToken);

        var consolidatedItems = flatItems
            .GroupBy(x => new { x.ProductId, x.ProductCode, x.ProductName, x.Unit })
            .Select(g => new
            {
                productId = g.Key.ProductId,
                productCode = g.Key.ProductCode,
                productName = g.Key.ProductName,
                unit = g.Key.Unit,
                totalPhysicalQuantity = g.Sum(x => x.PhysicalQuantity),
                totalAllocatedQuantity = g.Sum(x => x.AllocatedQuantity),
                totalSalableQuantity = g.Sum(x => x.PhysicalQuantity - x.AllocatedQuantity),
                warehouses = g.Select(x => new
                {
                    warehouseName = x.WarehouseName,
                    physicalQuantity = x.PhysicalQuantity,
                    allocatedQuantity = x.AllocatedQuantity,
                    salableQuantity = x.PhysicalQuantity - x.AllocatedQuantity,
                    brandZone = x.BrandZone,
                    rackCode = x.RackCode,
                    lastSyncAt = x.LastSyncAt
                }).ToList()
            })
            .ToList();

        return TypedResults.Ok(new { items = consolidatedItems, totalCount = consolidatedItems.Count });
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