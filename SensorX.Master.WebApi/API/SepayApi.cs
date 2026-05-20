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

        group.MapPost("/webhooks", async ([FromServices] IMediator mediator, HandlerPaymentSepayCommand command) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? Results.Created(result)
                : Results.BadRequest(result);
        })
            .WithName("HandleSepayPayment")
            .WithDescription("Handle Sepay payment webhook");
        return app;
    }
}