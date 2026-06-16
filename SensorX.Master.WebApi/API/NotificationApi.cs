using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using MediatR;
using SensorX.Master.Application.Commands.Notifications;
using SensorX.Master.Application.Queries.Notifications;
using SensorX.Master.WebApi.Extensions;

namespace SensorX.Master.WebApi.API;

public static class NotificationApi
{
    public static IEndpointRouteBuilder MapNotificationApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/notifications")
            .WithTags("Notifications")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("/", GetNotifications)
            .WithName("GetNotifications")
            .WithDescription("Get paged notifications for the current authenticated user");

        group.MapGet("/unread-count", GetUnreadCount)
            .WithName("GetUnreadCount")
            .WithDescription("Get unread notifications count for the current authenticated user");

        group.MapPut("/{notificationId:guid}/read", MarkAsRead)
            .WithName("MarkAsRead")
            .WithDescription("Mark a specific notification as read");

        group.MapPost("/read-all", MarkAllAsRead)
            .WithName("MarkAllAsRead")
            .WithDescription("Mark all notifications for the current user as read");

        return app;
    }

    private static async Task<IResult> GetNotifications(
        [AsParameters] GetNotificationsQuery query,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(query);
        return result.ToResult();
    }

    private static async Task<IResult> GetUnreadCount(
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new GetUnreadCountQuery());
        return result.ToResult();
    }

    private static async Task<IResult> MarkAsRead(
        [FromRoute] Guid notificationId,
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new MarkAsReadCommand(notificationId));
        return result.ToResult();
    }

    private static async Task<IResult> MarkAllAsRead(
        [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new MarkAllAsReadCommand());
        return result.ToResult();
    }
}
