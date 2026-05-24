using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Events.DomainEvents.RFQ.RFQForceAssigned;

/// <summary>
/// Lắng nghe RFQForceAssignedEvent (phát ra khi Manager chỉ định tay).
/// - Nếu có nhân viên cũ (PreviousStaffId != null): Release workload của họ.
/// - Tăng CurrentWorkload của nhân viên mới được chỉ định.
/// </summary>
public sealed class RFQForceAssignedEventHandler(
    IRepository<SaleStaff> _staffRepository
) : INotificationHandler<DomainEventNotification<RFQForceAssignedEvent>>
{
    public async Task Handle(DomainEventNotification<RFQForceAssignedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        // Bước 1: Release workload nhân viên cũ (nếu có)
        if (domainEvent.PreviousStaffId != null)
        {
            var previousStaff = await _staffRepository.GetByIdAsync(domainEvent.PreviousStaffId, cancellationToken);
            if (previousStaff != null)
            {
                previousStaff.ReleaseWorkload();
            }
        }

        // Bước 2: Tăng workload nhân viên mới
        var newStaff = await _staffRepository.GetByIdAsync(domainEvent.NewStaffId, cancellationToken)
            ?? throw new InvalidOperationException($"Không tìm thấy SaleStaff {domainEvent.NewStaffId.Value}");

        newStaff.AssignRfq();
        await _staffRepository.SaveChangesAsync(cancellationToken);
    }
}
