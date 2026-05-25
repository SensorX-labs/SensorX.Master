using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Events.DomainEvents.RFQ.RFQAssigned;

/// <summary>
/// Lắng nghe RFQAssignedEvent (phát ra khi AI phân bổ thành công).
/// Tăng CurrentWorkload của nhân viên được chọn.
/// </summary>
public sealed class RFQAssignedEventHandler(
    IRepository<SaleStaff> _staffRepository
) : INotificationHandler<DomainEventNotification<RFQAssignedEvent>>
{
    public async Task Handle(DomainEventNotification<RFQAssignedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        var staff = await _staffRepository.GetByIdAsync(domainEvent.StaffId, cancellationToken)
            ?? throw new InvalidOperationException($"Không tìm thấy SaleStaff {domainEvent.StaffId.Value}");

        staff.AssignRfq();
        await _staffRepository.SaveChangesAsync(cancellationToken);
    }
}
