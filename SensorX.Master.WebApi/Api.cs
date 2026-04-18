using SensorX.Master.WebApi;
using SensorX.Master.WebApi.API;

namespace SensorX.Master.WebApi;

public static class Api
{
    public static RouteGroupBuilder MapApi(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("api");
        // viết api cho master
        api.MapRFQApi();
        return api;
    }
}