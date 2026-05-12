using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SensorX.Master.Application.Queries.RFQs.GetPageListRFQ;
using SensorX.Master.Application.Queries.RFQs.GetRFQById;
using SensorX.Master.WebApi.Extensions;

namespace SensorX.Master.WebApi.API.Queries
{
    public static class RFQQueriesApi
    {
        public static RouteGroupBuilder MapRFQQueriesApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("rfq").WithTags("RFQ");

            api.MapGet("{id:guid}", GetRFQById).WithOpenApi(operation =>
            {
                operation.Summary = "Lấy chi tiết yêu cầu báo giá (RFQ)";
                return operation;
            });

            api.MapGet("", GetPageListRFQ).WithOpenApi(operation =>
            {
                operation.Summary = "Lấy danh sách RFQ có phân trang và tìm kiếm";
                return operation;
            });

            return api;
        }

        private static async Task<IResult> GetRFQById(
            [FromRoute] Guid id,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(new GetRFQByIdQuery(id));
            return result.ToResult();
        }

        private static async Task<IResult> GetPageListRFQ(
            [AsParameters] GetPageListRFQQuery query,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(query);
            return result.ToResult();
        }
    }
}
