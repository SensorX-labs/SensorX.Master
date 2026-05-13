using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Queries.RFQs.GetMyRFQPage;
using SensorX.Master.Application.Queries.RFQs.GetMyRFQPageDetail;
using SensorX.Master.Application.Queries.RFQs.GetPageListRFQ;
using SensorX.Master.Application.Queries.RFQs.GetRFQById;
using SensorX.Master.WebApi.Configurations;
using SensorX.Master.WebApi.Extensions;

namespace SensorX.Master.WebApi.API.Queries
{
    public static class RFQQueriesApi
    {
        public static RouteGroupBuilder MapRFQQueriesApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("rfq").WithTags("RFQ Query Api");

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

            api.MapGet("my-rfq", GetMyRFQPage).WithOpenApi(operation =>
            {
                operation.Summary = "Lấy danh sách RFQ của tôi (Trang của khách hàng)";
                return operation;
            });

            api.MapGet("my-rfq/{id:guid}", GetMyRFQPageDetail).WithOpenApi(operation =>
            {
                operation.Summary = "Lấy chi tiết RFQ của tôi (Trang của khách hàng)";
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

        [AuthorizeRole(Role.Customer)]
        private static async Task<IResult> GetMyRFQPage(
            [AsParameters] GetMyRFQPageQuery query,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(query);
            return result.ToResult();
        }
        [AuthorizeRole(Role.Customer)]
        private static async Task<IResult> GetMyRFQPageDetail(
            [FromRoute] Guid id,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(new GetMyRFQPageDetailQuery(id));
            return result.ToResult();
        }
    }
}
