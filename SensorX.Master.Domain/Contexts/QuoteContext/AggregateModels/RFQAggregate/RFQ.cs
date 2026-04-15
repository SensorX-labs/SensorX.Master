using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate
{
    public class RFQ : Entity<RFQId>, IAggregateRoot, ICreationTrackable, IUpdateTrackable
    {
        private RFQ() : base() { }
        public RFQ(
            RFQId id,
            Code code,
            StaffId staffId,
            CustomerId customerId,
            CustomerInfo customerInfo,
            RFQStatus status
        ) : base(id)
        {
            Code = code;
            StaffId = staffId;
            CustomerId = customerId;
            CustomerInfo = customerInfo;
            Status = status;
        }

        public Code Code { get; private set; }
        public StaffId StaffId { get; private set; }
        public CustomerId CustomerId { get; private set; }
        public CustomerInfo CustomerInfo { get; private set; }
        public RFQStatus Status { get; private set; }
        private readonly List<RFQItem> _items = new();
        public IReadOnlyList<RFQItem> Items => _items.AsReadOnly();
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}