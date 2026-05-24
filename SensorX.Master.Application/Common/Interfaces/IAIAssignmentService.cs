using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Common.Interfaces;

public interface IAIAssignmentService
{
    Task<SensorX.Master.Domain.StrongIDs.StaffId?> FindBestStaffForRFQAsync(RFQ rfq, CancellationToken cancellationToken = default);
}
