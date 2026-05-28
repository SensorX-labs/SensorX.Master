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

        group.MapPost("/webhooks", async (
            [FromServices] IMediator mediator, 
            [FromServices] IConfiguration configuration,
            HttpRequest httpRequest,
            [FromBody] HandlerPaymentSepayCommand command) =>
        {
            if (!httpRequest.Headers.TryGetValue("Authorization", out var authHeaderValues))
            {
                return Results.Unauthorized();
            }

            var authHeader = authHeaderValues.ToString();
            var prefix = "Apikey ";
            if (!authHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return Results.Unauthorized();
            }

            var apiKey = authHeader.Substring(prefix.Length).Trim();
            var configuredKey = configuration["Sepay:ApiKey"];

            if (string.IsNullOrEmpty(configuredKey) || apiKey != configuredKey)
            {
                return Results.Unauthorized();
            }

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