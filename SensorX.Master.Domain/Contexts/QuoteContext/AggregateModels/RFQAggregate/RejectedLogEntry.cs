using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate
{
    public record RejectedLogEntry(StaffId StaffId, string Reason, DateTimeOffset RejectedAt);
}
