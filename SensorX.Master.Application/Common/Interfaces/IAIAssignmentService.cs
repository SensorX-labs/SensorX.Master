using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Common.Interfaces;

public interface IAIAssignmentService
{
    Task AssignStaffToRFQAsync(RFQ rfq, CancellationToken cancellationToken = default);
}
