using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using SensorX.Master.Application.Commands.RFQs.CustomerCreateRFQ;
using SensorX.Master.Application.Commands.RFQs.CustomerSendRFQ;
using SensorX.Master.Application.Commands.RFQs.ManagerForceAssignRFQ;
using SensorX.Master.Application.Commands.RFQs.StaffAcceptRFQ;
using SensorX.Master.Application.Commands.RFQs.StaffRejectRFQ;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.WebApi.Configurations;
using SensorX.Master.WebApi.Extensions;

namespace SensorX.Master.WebApi.API.Commands
{
    public static class RFQCommandApi
    {
        public static RouteGroupBuilder MapRFQCommandApi(this IEndpointRouteBuilder app)
        {
            var api = app.MapGroup("rfq").WithTags("RFQ Command Api");

            api.MapPost("", CreateRFQ).WithOpenApi(operation =>
            {
                operation.Summary = "Khách hàng tạo yêu cầu báo giá nháp (Draft)";
                return operation;
            });

            api.MapPost("send", SendRFQ).WithOpenApi(operation =>
            {
                operation.Summary = "Khách hàng gửi yêu cầu báo giá";
                return operation;
            });

            api.MapPost("force-assign", ForceAssignRFQ).WithOpenApi(operation =>
            {
                operation.Summary = "Quản lý chỉ định nhân viên xử lý RFQ";
                return operation;
            });

            api.MapPost("accept", AcceptRFQ).WithOpenApi(operation =>
            {
                operation.Summary = "Nhân viên tiếp nhận xử lý RFQ";
                return operation;
            });

            api.MapPost("reject", RejectRFQ).WithOpenApi(operation =>
            {
                operation.Summary = "Nhân viên từ chối xử lý RFQ";
                return operation;
            });

            return api;
        }

        [AuthorizeRole(Role.Customer)]
        private static async Task<IResult> CreateRFQ(
            [FromBody] CustomerCreateRFQCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
            return result.ToResult();
        }

        [AuthorizeRole(Role.Customer)]
        private static async Task<IResult> SendRFQ(
            [FromBody] CustomerSendRFQCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
            return result.ToResult();
        }

        [AuthorizeRole(Role.Manager)]
        private static async Task<IResult> ForceAssignRFQ(
            [FromBody] ManagerForceAssignRFQCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
            return result.ToResult();
        }

        [AuthorizeRole(Role.SaleStaff)]
        private static async Task<IResult> AcceptRFQ(
            [FromBody] StaffAcceptRFQCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
            return result.ToResult();
        }

        [AuthorizeRole(Role.SaleStaff)]
        private static async Task<IResult> RejectRFQ(
            [FromBody] StaffRejectRFQCommand command,
            [FromServices] IMediator mediator
        )
        {
            var result = await mediator.Send(command);
            return result.ToResult();
        }
    }
}
