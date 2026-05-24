namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate
{
    public record AllocationLogEntry
    {
        public int Round { get; private set; }
        public DateTimeOffset AssignedAt { get; private set; }
        public string SnapshotJson { get; private set; }

#pragma warning disable CS8618 // EF Core requires parameterless constructor
        private AllocationLogEntry() { }
#pragma warning restore CS8618

        public AllocationLogEntry(int round, DateTimeOffset assignedAt, string snapshotJson)
        {
            Round = round;
            AssignedAt = assignedAt;
            SnapshotJson = snapshotJson;
        }
    }
}
