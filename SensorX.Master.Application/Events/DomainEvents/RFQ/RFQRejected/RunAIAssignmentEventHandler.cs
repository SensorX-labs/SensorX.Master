using MediatR;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.Common.Exceptions;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Events.DomainEvents.RFQ.RFQRejected;

public class RunAIAssignmentEventHandler(
    IAIAssignmentService _aiAssignmentService,
    IRepository<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate.RFQ> _rfqRepository,
    IRepository<SensorX.Master.Application.Common.ReadModel.SaleStaff> _staffRepository,
    ILogger<RunAIAssignmentEventHandler> _logger
) : INotificationHandler<DomainEventNotification<RFQRejectedEvent>>
{
    public async Task Handle(DomainEventNotification<RFQRejectedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogInformation("Nhận sự kiện RFQRejectedEvent cho RFQ {Id} (Staff {StaffId} từ chối). Phân bổ lại...", domainEvent.RfqId.Value, domainEvent.StaffId?.Value);

        var rfq = await _rfqRepository.GetByIdAsync(domainEvent.RfqId, cancellationToken)
        ?? throw new SensorX.Master.Application.Common.Exceptions.ApplicationException("RFQ không tồn tại.");

        var bestStaffId = await _aiAssignmentService.FindBestStaffForRFQAsync(rfq, cancellationToken);
        if (bestStaffId != null)
        {
            rfq.Assign(bestStaffId);
            await _rfqRepository.SaveChangesAsync(cancellationToken);

            var dbStaff = await _staffRepository.GetByIdAsync(bestStaffId, cancellationToken);
            if (dbStaff != null)
            {
                dbStaff.AssignRfq();
                await _staffRepository.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
