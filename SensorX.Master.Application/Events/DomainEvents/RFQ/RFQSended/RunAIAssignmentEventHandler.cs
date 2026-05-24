using MediatR;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.Common.Exceptions;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Events.DomainEvents.RFQ.RFQSended;

public class RunAIAssignmentEventHandler(
    IAIAssignmentService _aiAssignmentService,
    IRepository<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate.RFQ> _rfqRepository,
    ILogger<RunAIAssignmentEventHandler> _logger
) : INotificationHandler<DomainEventNotification<RFQSendedEvent>>
{
    public async Task Handle(DomainEventNotification<RFQSendedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogInformation("Nhận sự kiện RFQSendedEvent cho RFQ {Id}, tiến hành phân bổ AI...", domainEvent.RfqId.Value);

        var rfq = await _rfqRepository.GetByIdAsync(domainEvent.RfqId, cancellationToken);
        if (rfq is null) throw new DomainException("RFQ không tồn tại.");

        await _aiAssignmentService.AssignStaffToRFQAsync(rfq, cancellationToken);
        await _rfqRepository.SaveChangesAsync(cancellationToken);
    }
}
