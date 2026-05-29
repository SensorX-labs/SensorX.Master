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
<<<<<<< HEAD
        api.MapAnalyticsQueriesApi();
=======
        api.MapSepayApi();
>>>>>>> 39629a4873ae7a7541763026a0684a133f154091
        return api;
    }
}
