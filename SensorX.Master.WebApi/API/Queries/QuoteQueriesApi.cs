using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Queries.Quotes.GetDetailQuoteById;
using SensorX.Master.Application.Queries.Quotes.GetMyQuotes;
using SensorX.Master.Application.Queries.Quotes.GetPageListQuote;
using SensorX.Master.Application.Queries.Quotes.GetQuoteStats;
using SensorX.Master.WebApi.Configurations;
using SensorX.Master.WebApi.Extensions;

namespace SensorX.Master.WebApi.API.Queries
{
    public static class QuoteQueriesApi
    {
        public static IEndpointRouteBuilder MapQuoteQueriesApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("quotes").WithTags("Quotes Queries");

            api.MapGet("{quoteId:guid}", GetDetailQuoteById).WithOpenApi(operation =>
            {
                operation.Summary = "Lấy chi tiết báo giá";
                return operation;
            });

            api.MapGet("stats", GetQuoteStats).WithOpenApi(operation =>
            {
                operation.Summary = "Lấy thống kê báo giá";
                return operation;
            });

            api.MapGet("", GetPageListQuote).WithOpenApi(operation =>
            {
                operation.Summary = "Lấy danh sách báo giá có phân trang";
                return operation;
            });

            api.MapGet("my-quotes", GetMyQuotes).WithOpenApi(operation =>
            {
                operation.Summary = "Lấy danh sách báo giá của tôi (Trang của khách hàng)";
                return operation;
            });

            api.MapGet("my-quote/{quoteId:guid}", GetMyQuoteDetail).WithOpenApi(operation =>
            {
                operation.Summary = "Lấy chi tiết báo giá của tôi (Trang của khách hàng)";
                return operation;
            });

            return api;
        }

        [AuthorizeRole(Role.SaleStaff, Role.Manager)]
        private static async Task<IResult> GetDetailQuoteById(
            [FromRoute] Guid quoteId,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(new GetDetailQuoteByIdQuery(quoteId));
            return result.ToResult();
        }

        [AuthorizeRole(Role.SaleStaff, Role.Manager)]
        private static async Task<IResult> GetQuoteStats(
            [AsParameters] GetQuoteStatsQuery query,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(query);
            return result.ToResult();
        }

        [AuthorizeRole(Role.SaleStaff, Role.Manager)]
        private static async Task<IResult> GetPageListQuote(
            [AsParameters] GetPageListQuoteQuery query,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(query);
            return result.ToResult();
        }

        [AuthorizeRole(Role.Customer)]
        private static async Task<IResult> GetMyQuotes(
            [AsParameters] GetMyQuotesQuery query,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(query);
            return result.ToResult();
        }
        [AuthorizeRole(Role.Customer)]
        private static async Task<IResult> GetMyQuoteDetail(
            [FromRoute] Guid quoteId,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(new SensorX.Master.Application.Queries.Quotes.GetMyQuoteDetail.GetMyQuoteDetailQuery(quoteId));
            return result.ToResult();
        }
    }
}