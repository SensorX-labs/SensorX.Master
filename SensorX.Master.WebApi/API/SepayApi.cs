using Microsoft.AspNetCore.Mvc;
using MediatR;
using SensorX.Master.Application.Commands.Sepays;
using SensorX.Master.Application.Queries.PaymentHistories.GetPageListPaymentHistory;
using SensorX.Master.Application.Queries.PaymentHistories.GetDetailPaymentHistory;
using SensorX.Master.WebApi.Extensions;

namespace SensorX.Master.WebApi.API;

public static class SepayApi
{
    public static IEndpointRouteBuilder MapSepayApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/sepay")
            .WithTags("Sepay")
            .WithOpenApi();

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

        group.MapGet("/history", GetPagedListPaymentHistory)
            .WithName("GetPagedListPaymentHistory")
            .WithDescription("Get paged list of payment histories");

        group.MapGet("/history/{id:int}", GetDetailPaymentHistory)
            .WithName("GetDetailPaymentHistory")
            .WithDescription("Get payment history detail by ID");

        return app;
    }

    private static async Task<IResult> GetPagedListPaymentHistory(
        [AsParameters] GetPageListPaymentHistoryQuery query,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(query);
        return result.ToResult();
    }

    private static async Task<IResult> GetDetailPaymentHistory(
        [FromRoute] int id,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new GetDetailPaymentHistoryQuery(id));
        return result.ToResult();
    }
}