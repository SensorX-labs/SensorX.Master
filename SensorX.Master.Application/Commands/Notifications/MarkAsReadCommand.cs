using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.Notifications;

public record MarkAsReadCommand(Guid NotificationId) : IRequest<Result>;

public class MarkAsReadHandler(
    INotificationRepository notificationRepository,
    ICurrentUser currentUser
) : IRequestHandler<MarkAsReadCommand, Result>
{
    public async Task<Result> Handle(MarkAsReadCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result.Failure("User is not authenticated");

        await notificationRepository.MarkAsReadAsync(request.NotificationId, currentUser.UserId.Value, cancellationToken);
        await notificationRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
