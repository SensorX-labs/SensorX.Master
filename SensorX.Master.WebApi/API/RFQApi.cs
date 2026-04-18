using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.Commands.RFQs.CreateRFQ;
using SensorX.Master.Application.Commands.RFQs.AssignRFQ;
using SensorX.Master.Application.Commands.RFQs.AcceptRFQ;
using SensorX.Master.Application.Commands.RFQs.RejectRFQ;

namespace SensorX.Master.WebApi.API
{
    public static class RFQApi
    {
        public static RouteGroupBuilder MapRFQApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("rfq").WithTags("RFQ");

            api.MapPost("", CreateRFQ).WithOpenApi();
            api.MapPost("assign", AssignRFQ).WithOpenApi();
            api.MapPost("accept", AcceptRFQ).WithOpenApi();
            api.MapPost("reject", RejectRFQ).WithOpenApi();
            return api;
        }

        private static async Task<Results<Ok<Result<Guid>>, BadRequest<string>>> CreateRFQ(
            [FromBody] CreateRFQCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
            return result.IsSuccess 
                ? TypedResults.Ok(result) 
                : TypedResults.BadRequest(result.Error ?? "Lỗi khi tạo RFQ");
        }

        private static async Task<Results<Ok<Result<Guid>>, BadRequest<string>>> AssignRFQ(
            [FromBody] AssignRFQCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
            return result.IsSuccess 
                ? TypedResults.Ok(result) 
                : TypedResults.BadRequest(result.Error ?? "Lỗi khi gán RFQ");
        }

        private static async Task<Results<Ok<Result<Guid>>, BadRequest<string>>> AcceptRFQ(
            [FromBody] AcceptRFQCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
            return result.IsSuccess 
                ? TypedResults.Ok(result) 
                : TypedResults.BadRequest(result.Error ?? "Lỗi khi duyệt RFQ");
        }

        private static async Task<Results<Ok<Result<Guid>>, BadRequest<string>>> RejectRFQ(
            [FromBody] RejectRFQCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
            return result.IsSuccess 
                ? TypedResults.Ok(result) 
                : TypedResults.BadRequest(result.Error ?? "Lỗi khi từ chối RFQ");
        }
    }
}