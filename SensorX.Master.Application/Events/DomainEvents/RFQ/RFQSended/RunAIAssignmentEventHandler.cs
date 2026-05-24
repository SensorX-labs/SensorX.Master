using MediatR;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.Common.Exceptions;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

using SensorX.Master.Application.Services.AIAssignment;
using System.Text.Json;

namespace SensorX.Master.Application.Events.DomainEvents.RFQ.RFQSended;

public class RunAIAssignmentEventHandler(
    IAIAssignmentService _aiAssignmentService,
    IRepository<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate.RFQ> _rfqRepository,
    IRepository<SensorX.Master.Application.Common.ReadModel.SaleStaff> _staffRepository,
    IUnitOfWork _unitOfWork,
    ILogger<RunAIAssignmentEventHandler> _logger
) : INotificationHandler<DomainEventNotification<RFQSendedEvent>>
{
    public async Task Handle(DomainEventNotification<RFQSendedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        _logger.LogInformation("Nhận sự kiện RFQSendedEvent cho RFQ {Id}, tiến hành phân bổ AI...", domainEvent.RfqId.Value);

        var rfq = await _rfqRepository.GetByIdAsync(domainEvent.RfqId, cancellationToken);
        if (rfq is null)
        {
            _logger.LogError("RFQ {Id} không tồn tại", domainEvent.RfqId.Value);
            throw new Exception("RFQ không tồn tại");
        }

        if (rfq.Status != RFQStatus.Pending)
        {
            _logger.LogError("RFQ {Id} không ở trạng thái chờ phân bổ", domainEvent.RfqId.Value);
            throw new Exception("RFQ không ở trạng thái chờ phân bổ");
        }
        if (rfq.Items.Count == 0 || rfq.Items == null)
        {
            _logger.LogError("RFQ {Id} không có sản phẩm", domainEvent.RfqId.Value);
            throw new Exception("RFQ không có sản phẩm");
        }

        var allocationResult = await _aiAssignmentService.FindBestStaffForRFQAsync(rfq, cancellationToken);
        if (allocationResult.WinnerStaffId != null)
        {
            string logJson = JsonSerializer.Serialize(allocationResult.CandidatesSnapshot);
            rfq.Assign(allocationResult.WinnerStaffId, logJson);
            var dbStaff = await _staffRepository.GetByIdAsync(allocationResult.WinnerStaffId, cancellationToken) ?? throw new Exception("Nhân viên không tồn tại");
            dbStaff?.AssignRfq();
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Phân bổ AI thành công cho RFQ {Id}", domainEvent.RfqId.Value);
    }
}
