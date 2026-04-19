using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SensorX.Master.Application.Commands.Quotes.CreateQuote;
using SensorX.Master.Application.Queries.Quotes.GetDetailQuoteById;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.WebApi.API
{
    public static class QuoteApi
    {
        public static IEndpointRouteBuilder MapQuoteApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("quotes").WithTags("Quotes");

            api.MapPost("", CreateQuote).WithOpenApi(operation => {
                operation.Summary = "Tạo báo giá mới";
                operation.Description = "Tạo báo giá (Draft) dựa trên thông tin gửi xuống từ Frontend (kế thừa từ RFQ).";
                return operation;
            });

            api.MapGet("{quoteId:guid}", GetDetailQuoteById).WithOpenApi(operation => {
                operation.Summary = "Lấy chi tiết báo giá";
                return operation;
            });

            return api;
        }

        private static async Task<Results<Ok<Result<Guid>>, BadRequest<string>>> CreateQuote(
            [FromBody] CreateQuoteCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
            return result.IsSuccess 
                ? TypedResults.Ok(result) 
                : TypedResults.BadRequest(result.Error ?? "Lỗi khi tạo báo giá");
        }

        private static async Task<Results<Ok<Result<GetDetailQuoteByIdResponse>>, BadRequest<string>>> GetDetailQuoteById(
            [FromRoute] Guid quoteId,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(new GetDetailQuoteByIdQuery(quoteId));
            return result.IsSuccess 
                ? TypedResults.Ok(result) 
                : TypedResults.BadRequest(result.Error ?? "Lỗi khi lấy chi tiết báo giá");
        }
    }
}
