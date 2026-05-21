using Microsoft.AspNetCore.Mvc;
using MediatR;
using SensorX.Master.Application.Commands.Sepays;

namespace SensorX.Master.WebApi.API;

public static class SepayApi
{
    public static IEndpointRouteBuilder MapSepayApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/sepay");

        group.WithOpenApi();

        group.MapPost("/webhooks", async ([FromServices] IMediator mediator, [FromBody] HandlerPaymentSepayCommand command) =>
        {
            var result = await mediator.Send(command);
            return result
                ? Results.Ok(new { success = true })
                : Results.BadRequest(new { success = false });
        })
            .AllowAnonymous()
            .WithName("HandleSepayPayment")
            .WithDescription("Handle Sepay payment webhook")
            .DisableAntiforgery();
        return app;
    }
}