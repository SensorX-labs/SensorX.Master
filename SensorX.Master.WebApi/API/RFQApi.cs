using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.Commands.RFQs.CreateRFQ;

namespace SensorX.Master.WebApi.API
{
    public static class RFQApi
    {
        public static RouteGroupBuilder MapRFQApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("rfq").WithTags("RFQ");

            api.MapPost("", CreateRFQ).WithOpenApi();
            
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
    }
}