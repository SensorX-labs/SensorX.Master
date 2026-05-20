using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SensorX.Master.Application.Commands.Quotes.CustomerRespondToQuote;
using SensorX.Master.Application.Commands.Quotes.ApproveQuote;
using SensorX.Master.Application.Commands.Quotes.CreateDraftQuote;
using SensorX.Master.Application.Commands.Quotes.SubmitQuoteForApproval;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Queries.Quotes.GetDetailQuoteById;
using SensorX.Master.Application.Queries.Quotes.GetMyQuotes;
using SensorX.Master.Application.Queries.Quotes.GetPageListQuote;
using SensorX.Master.WebApi.Configurations;
using SensorX.Master.WebApi.Extensions;

namespace SensorX.Master.WebApi.API.Commands
{
    public static class QuoteCommandApi
    {
        public static IEndpointRouteBuilder MapQuoteCommandApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("quotes").WithTags("Quotes Commands");

            api.MapPost("", CreateDraftQuote).WithOpenApi(operation =>
            {
                operation.Summary = "Tạo bản thảo báo giá mới";
                operation.Description = "Tạo báo giá (Draft) dựa trên thông tin gửi xuống từ Frontend (kế thừa từ RFQ).";
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

        private static async Task<IResult> CreateDraftQuote(
            [FromBody] CreateDraftQuoteCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
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
            [FromBody] CustomerRespondToQuoteCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command.WithId(quoteId));
            return result.ToResult();
        }
    }
}