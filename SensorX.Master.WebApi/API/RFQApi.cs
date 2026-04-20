using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using SensorX.Master.Application.Commands.RFQs.AcceptRFQ;
using SensorX.Master.Application.Commands.RFQs.AssignRFQ;
using SensorX.Master.Application.Commands.RFQs.CreateRFQ;
using SensorX.Master.Application.Commands.RFQs.RejectRFQ;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.Queries.RFQs.GetPageListRFQ;
using SensorX.Master.Application.Queries.RFQs.GetRFQById;

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

        private static async Task<Results<Ok<Result<GetRFQByIdResponse>>, BadRequest<string>>> GetRFQById(
            [FromRoute] Guid id,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(new GetRFQByIdQuery(id));
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result.Error ?? "Lỗi khi lấy RFQ");
        }

        private static async Task<Results<Ok<Result<RFQCursorPagedResult>>, BadRequest<string>>> GetPageListRFQ(
            [AsParameters] GetPageListRFQQuery query,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result.Error ?? "Lỗi khi lấy danh sách RFQ");
        }
    }
}