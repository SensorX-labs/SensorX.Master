using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Commands.Warehouses;
using SensorX.Master.Application.DTOs;
using SensorX.Master.Application.Queries.Warehouses;
using SensorX.Master.Application.Services;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;

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
        [FromServices] IHttpClientFactory httpClientFactory,
        [FromServices] ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("WarehouseApi");
        var warehouses = await warehouseQueryService.GetAllAsync(cancellationToken);
        var activeWarehouses = warehouses.Where(w => w.IsActive).ToList();

        var client = httpClientFactory.CreateClient();
        var allItems = new List<object>();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        foreach (var warehouse in activeWarehouses)
        {
            try
            {
                var baseUrl = warehouse.ApiEndpointUrl.TrimEnd('/');
                var url = $"{baseUrl}/api/inventory/list?pageSize=1000";
                
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("X-Warehouse-Id", warehouse.Id.ToString());

                var response = await client.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    using var doc = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationToken);
                    
                    if (doc.RootElement.TryGetProperty("items", out var itemsElement) && itemsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in itemsElement.EnumerateArray())
                        {
                            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(item.GetRawText(), options) ?? new Dictionary<string, object>();
                            dict["warehouseName"] = warehouse.Name;
                            allItems.Add(dict);
                        }
                    }
                }
                else
                {
                    logger.LogWarning("Failed to fetch inventory from warehouse {Name} at {Url}. Status: {Status}", warehouse.Name, url, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error fetching inventory from warehouse {Name}", warehouse.Name);
            }
        }

        return TypedResults.Ok(new { items = allItems, totalCount = allItems.Count });
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