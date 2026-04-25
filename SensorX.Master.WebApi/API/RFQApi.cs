using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SensorX.Master.Application.Commands.RFQs.AcceptRFQ;
using SensorX.Master.Application.Commands.RFQs.AssignRFQ;
using SensorX.Master.Application.Commands.RFQs.CreateRFQ;
using SensorX.Master.Application.Commands.RFQs.RejectRFQ;
using SensorX.Master.Application.Queries.RFQs.GetPageListRFQ;
using SensorX.Master.Application.Queries.RFQs.GetRFQById;
using SensorX.Master.WebApi.Extensions;

namespace SensorX.Master.WebApi.API
{
    public static class RFQApi
    {
        public static RouteGroupBuilder MapRFQApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("rfq").WithTags("RFQ");

            api.MapPost("", CreateRFQ).WithOpenApi(operation =>
            {
                operation.Summary = "Tạo yêu cầu báo giá mới";
                return operation;
            });
            api.MapPost("assign", AssignRFQ).WithOpenApi(operation =>
            {
                operation.Summary = "Gán nhân viên xử lý RFQ";
                return operation;
            });
            api.MapPost("accept", AcceptRFQ).WithOpenApi(operation =>
            {
                operation.Summary = "Chấp nhận RFQ";
                return operation;
            });
            api.MapPost("reject", RejectRFQ).WithOpenApi(operation =>
            {
                operation.Summary = "Từ chối RFQ";
                return operation;
            });
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

        private static async Task<IResult> CreateRFQ(
            [FromBody] CreateRFQCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
            return result.ToResult();
        }

        private static async Task<IResult> AssignRFQ(
            [FromBody] AssignRFQCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
            return result.ToResult();
        }

        private static async Task<IResult> AcceptRFQ(
            [FromBody] AcceptRFQCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
            return result.ToResult();
        }

        private static async Task<IResult> RejectRFQ(
            [FromBody] RejectRFQCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
            return result.ToResult();
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