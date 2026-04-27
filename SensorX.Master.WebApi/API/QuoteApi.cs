using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SensorX.Master.Application.Commands.Quotes.AcceptQuote;
using SensorX.Master.Application.Commands.Quotes.ApproveQuote;
using SensorX.Master.Application.Commands.Quotes.SubmitQuoteForApproval;
using SensorX.Master.Application.Commands.Quotes.CreateQuote;
using SensorX.Master.Application.Queries.Quotes.GetDetailQuoteById;
using SensorX.Master.Application.Queries.Quotes.GetPageListQuote;
using SensorX.Master.WebApi.Extensions;

namespace SensorX.Master.WebApi.API
{
    public static class QuoteApi
    {
        public static IEndpointRouteBuilder MapQuoteApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("quotes").WithTags("Quotes");

            api.MapPost("", CreateQuote).WithOpenApi(operation =>
            {
                operation.Summary = "Tạo báo giá mới";
                operation.Description = "Tạo báo giá (Draft) dựa trên thông tin gửi xuống từ Frontend (kế thừa từ RFQ).";
                return operation;
            });

            api.MapGet("{quoteId:guid}", GetDetailQuoteById).WithOpenApi(operation =>
            {
                operation.Summary = "Lấy chi tiết báo giá";
                return operation;
            });

            api.MapGet("", GetPageListQuote).WithOpenApi(operation =>
            {
                operation.Summary = "Lấy danh sách báo giá có phân trang";
                return operation;
            });

            api.MapPost("{quoteId:guid}/submit-for-approval", SubmitQuoteForApproval).WithOpenApi(operation =>
            {
                operation.Summary = "Gửi báo giá để chờ duyệt";
                return operation;
            });

            api.MapPost("{quoteId:guid}/approve", ApproveQuote).WithOpenApi(operation =>
            {
                operation.Summary = "Phê duyệt báo giá";
                return operation;
            });

            api.MapPost("{quoteId:guid}/accept", AcceptQuote).WithOpenApi(operation =>
            {
                operation.Summary = "Khách hàng chấp nhận báo giá";
                return operation;
            });

            return api;
        }

        private static async Task<IResult> CreateQuote(
            [FromBody] CreateQuoteCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
            return result.ToResult();
        }

        private static async Task<IResult> GetDetailQuoteById(
            [FromRoute] Guid quoteId,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(new GetDetailQuoteByIdQuery(quoteId));
            return result.ToResult();
        }

        private static async Task<IResult> GetPageListQuote(
            [AsParameters] GetPageListQuoteQuery query,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(query);
            return result.ToResult();
        }

        private static async Task<IResult> SubmitQuoteForApproval(
            [FromRoute] Guid quoteId,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(new SubmitQuoteForApprovalCommand(quoteId));
            return result.ToResult();
        }

        private static async Task<IResult> ApproveQuote(
            [FromRoute] Guid quoteId,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(new ApproveQuoteCommand(quoteId));
            return result.ToResult();
        }

        private static async Task<IResult> AcceptQuote(
            [FromRoute] Guid quoteId,
            [FromBody] AcceptQuoteCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command.WithId(quoteId));
            return result.ToResult();
        }
    }
}
