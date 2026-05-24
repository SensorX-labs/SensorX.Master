using SensorX.Master.Application.Services.AIAssignment.Models;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Services.AIAssignment;

public interface IAIAssignmentService
{
    Task<AIAllocationResult> FindBestStaffForRFQAsync(RFQ rfq, CancellationToken cancellationToken = default);
}
