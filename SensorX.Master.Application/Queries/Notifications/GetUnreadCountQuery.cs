using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Notifications;

public record GetUnreadCountQuery : IRequest<Result<int>>;

public class GetUnreadCountHandler(
    INotificationRepository notificationRepository,
    ICurrentUser currentUser
) : IRequestHandler<GetUnreadCountQuery, Result<int>>
{
    public async Task<Result<int>> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result<int>.Failure("User is not authenticated");

        var userRoleStr = currentUser.Role?.ToString();
        var count = await notificationRepository.GetUnreadCountAsync(currentUser.UserId.Value, userRoleStr, cancellationToken);
        return Result<int>.Success(count);
    }
}
