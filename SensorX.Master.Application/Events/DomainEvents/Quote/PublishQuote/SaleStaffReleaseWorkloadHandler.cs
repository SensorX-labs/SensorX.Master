using MediatR;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Events;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Events.DomainEvents.Quote.PublishQuote;

public sealed class SaleStaffReleaseWorkloadHandler(
    IRepository<SaleStaff> _saleStaffRepository
) : INotificationHandler<DomainEventNotification<PublishQuoteEvent>>
{
    public async Task Handle(
        DomainEventNotification<PublishQuoteEvent> notification,
        CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var saleStaff = await _saleStaffRepository.GetByIdAsync(domainEvent.StaffId, cancellationToken)
        ?? throw new InvalidOperationException($"Không tìm thấy SaleStaff {domainEvent.StaffId}");
        saleStaff.ReleaseWorkload();
        await _saleStaffRepository.SaveChangesAsync(cancellationToken);
    }
}