using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;
using SensorX.Master.Domain.Common.Exceptions;

namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate
{
    public class RFQ : Entity<RFQId>, IAggregateRoot, ICreationTrackable, IUpdateTrackable
    {
        private RFQ() : base() { }
        public RFQ(
            RFQId id,
            Code code,
            StaffId? staffId,
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
        public StaffId? StaffId { get; private set; }
        public CustomerId CustomerId { get; private set; }
        public CustomerInfo CustomerInfo { get; private set; }
        public RFQStatus Status { get; private set; }
        private readonly List<RFQItem> _items = new();
        public IReadOnlyList<RFQItem> Items => _items.AsReadOnly();

        // gán nhân viên
        public void Assign(StaffId staffId)
        {
            if (Status != RFQStatus.Pending)
                throw new DomainException("Chỉ có thể gán nhân viên khi RFQ đang chờ xử lý.");

            StaffId = staffId;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        // nhân viên chấp nhận xử lý
        public void Accept()
        {
            if (Status != RFQStatus.Pending || StaffId == null)
                throw new DomainException("Phải có nhân viên gán trước khi chấp nhận và RFQ phải đang chờ xử lý.");

            Status = RFQStatus.Accepted;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        // nhân viên từ chối xử lý
        public void StaffReject()
        {
            if (Status != RFQStatus.Pending)
                throw new DomainException("Chỉ có thể từ chối khi RFQ đang chờ xử lý.");

            StaffId = null;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        // tất cả nhân viên từ chối
        public void AllReject()
        {
            if (Status != RFQStatus.Pending)
                throw new DomainException("Chỉ chuyển sang Rejected khi đang ở trạng thái Pending.");

            Status = RFQStatus.Rejected;
            StaffId = null;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        // ép gán nhân viên từ trạng thái Rejected
        public void ForceAssign(StaffId staffId)
        {
            if (Status != RFQStatus.Rejected)
                throw new DomainException("Force Assign chỉ dùng khi RFQ đã bị từ chối.");

            StaffId = staffId;
            Status = RFQStatus.Accepted;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        // phản hồi RFQ bằng Báo giá
        public void Respond()
        {
            if (Status != RFQStatus.Accepted)
                throw new DomainException("Chỉ có thể phản hồi khi RFQ đã được tiếp nhận.");

            Status = RFQStatus.Responded;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        // chuyển đổi thành báo giá
        public void MarkAsConverted()
        {
            if (Status != RFQStatus.Responded)
                throw new DomainException("Chỉ có thể chuyển đổi khi RFQ đã được phản hồi.");

            Status = RFQStatus.Converted;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        // hủy bỏ RFQ
        public void Cancel()
        {
            if (Status != RFQStatus.Responded)
                throw new DomainException("Chỉ có thể hủy khi RFQ đã được phản hồi.");

            Status = RFQStatus.Cancelled;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void AddItem(RFQItem item)
        {
            _items.Add(item);
        }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}