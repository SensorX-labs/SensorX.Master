using MediatR;
using Microsoft.AspNetCore.Mvc;
using SensorX.Master.Application.Queries.Invoices.GetInvoiceById;
using SensorX.Master.Application.Queries.Invoices.GetInvoiceByOrderId;
using SensorX.Master.Application.Queries.Invoices.GetPagedListInvoice;
using SensorX.Master.WebApi.Extensions;

namespace SensorX.Master.WebApi.API;

public static class InvoiceApi
{
    public static IEndpointRouteBuilder MapInvoiceApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/invoices")
            .WithTags("Invoices")
            .WithOpenApi();

        group.MapGet("/", GetPagedListInvoice)
            .WithName("GetPagedListInvoices")
            .WithDescription("Get paged list of invoices");

        group.MapGet("/{invoiceId:guid}", GetInvoiceById)
            .WithName("GetInvoiceById")
            .WithDescription("Get invoice detail by ID");

        group.MapGet("/order/{orderId:guid}", GetInvoiceByOrderId)
            .WithName("GetInvoiceByOrderId")
            .WithDescription("Get invoice detail by order ID");

        return app;
    }

    private static async Task<IResult> GetPagedListInvoice(
        [AsParameters] GetPagedListInvoiceQuery query,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(query);
        return result.ToResult();
    }

    private static async Task<IResult> GetInvoiceById(
        [FromRoute] Guid invoiceId,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new GetInvoiceByIdQuery(invoiceId));
        return result.ToResult();
    }

    private static async Task<IResult> GetInvoiceByOrderId(
        [FromRoute] Guid orderId,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new GetInvoiceByOrderIdQuery(orderId));
        return result.ToResult();
    }
}
