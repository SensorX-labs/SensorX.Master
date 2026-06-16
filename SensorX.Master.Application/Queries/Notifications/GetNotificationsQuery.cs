using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.DTOs;

namespace SensorX.Master.Application.Queries.Notifications;

public record GetNotificationsQuery : OffsetPagedQuery, IRequest<Result<OffsetPagedResult<NotificationDto>>>;

public class GetNotificationsHandler(
    INotificationRepository notificationRepository,
    ICurrentUser currentUser
) : IRequestHandler<GetNotificationsQuery, Result<OffsetPagedResult<NotificationDto>>>
{
    public async Task<Result<OffsetPagedResult<NotificationDto>>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result<OffsetPagedResult<NotificationDto>>.Failure("User is not authenticated");

        var pageNumber = request.PageNumber ?? 1;
        var pageSize = request.PageSize ?? 20;
        var skip = (pageNumber - 1) * pageSize;
        var take = pageSize;

        var userRoleStr = currentUser.Role?.ToString();
        var notifications = await notificationRepository.GetByUserIdAsync(currentUser.UserId.Value, userRoleStr, skip, take, cancellationToken);
        var totalCount = await notificationRepository.GetUnreadCountAsync(currentUser.UserId.Value, userRoleStr, cancellationToken);

        var items = notifications.Select(n => new NotificationDto(
            n.Id,
            n.Title,
            n.Content,
            n.Type,
            n.TargetUrl,
            n.IsRead,
            n.CreatedAt
        )).ToList();

        return Result<OffsetPagedResult<NotificationDto>>.Success(new OffsetPagedResult<NotificationDto>
        {
            Items = items,
            TotalCount = totalCount + items.Count(i => i.IsRead), // Rough estimate for UI pagination
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }
}
