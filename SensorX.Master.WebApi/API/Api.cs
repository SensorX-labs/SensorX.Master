using System.Text.Json.Serialization;
using SensorX.Master.WebApi.API.Commands;
using SensorX.Master.WebApi.API.Queries;

namespace SensorX.Master.WebApi.API;

public static class Api
{
    public static RouteGroupBuilder MapApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api");
        // viết api cho master
        api.MapRFQQueriesApi();
        api.MapRFQCommandApi();
        api.MapQuoteQueriesApi();
        api.MapQuoteCommandApi();
        api.MapOrderApi();
        api.MapInvoiceApi();
        api.MapTransferOrderApi();
        api.MapWarehouseApi(); // Add Warehouse API
        api.MapSupplyRequestApi();
        api.MapAnalyticsQueriesApi();
        api.MapSepayApi();
        api.MapAIHyperparametersApi();
        api.MapStaffQueriesApi();
        api.MapNotificationApi();
        return api;
    }
}
