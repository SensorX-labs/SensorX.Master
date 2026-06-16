using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.Notifications;

public record MarkAllAsReadCommand : IRequest<Result>;

public class MarkAllAsReadHandler(
    INotificationRepository notificationRepository,
    ICurrentUser currentUser
) : IRequestHandler<MarkAllAsReadCommand, Result>
{
    public async Task<Result> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result.Failure("User is not authenticated");

        var userRoleStr = currentUser.Role?.ToString();
        await notificationRepository.MarkAllAsReadAsync(currentUser.UserId.Value, userRoleStr, cancellationToken);
        await notificationRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
