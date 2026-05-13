using SensorX.Master.Domain.Common.Exceptions;
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
            StaffId? staffId,
            CustomerId customerId,
            CustomerInfo customerInfo
        ) : base(id)
        {
            Code = code;
            StaffId = staffId;
            CustomerId = customerId;
            CustomerInfo = customerInfo;
            Status = RFQStatus.Draft;
        }

        public Code Code { get; private set; }
        public StaffId? StaffId { get; private set; }
        public CustomerId CustomerId { get; private set; }
        public CustomerInfo CustomerInfo { get; private set; }
        public RFQStatus Status { get; private set; }

        private readonly List<RFQItem> _items = [];
        public IReadOnlyList<RFQItem> Items => _items.AsReadOnly();

        // private readonly List<StaffId> _rejectedByStaffIds = [];
        // public IReadOnlyList<StaffId> RejectedByStaffIds => _rejectedByStaffIds.AsReadOnly();

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

        // khách hàng gửi RFQ
        public void Send()
        {
            if (Status != RFQStatus.Draft)
                throw new DomainException("RFQ đang ở trạng thái không hợp lệ.");

            Status = RFQStatus.Pending;
            UpdatedAt = DateTimeOffset.UtcNow;
            AddDomainEvent(new RFQSendedEvent(Id, Code));
        }

        // gán nhân viên để xử lý RFQ
        public void Assign(StaffId staffId)
        {
            if (Status != RFQStatus.Pending)
                throw new DomainException("Chỉ có thể phân bổ RFQ ở trạng thái chờ phân bổ");

            StaffId = staffId;
            UpdatedAt = DateTimeOffset.UtcNow;
            AddDomainEvent(new RFQAssignedEvent(Id, Code, StaffId));
        }

        // nhân viên chấp nhận xử lý
        public void Accept()
        {
            if (Status != RFQStatus.Pending || StaffId == null)
                throw new DomainException("Chỉ có thể tiếp nhận RFQ ở trạng thái chờ phân bổ");

            Status = RFQStatus.Accepted;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        // nhân viên từ chối xử lý
        public void Reject()
        {
            if (Status != RFQStatus.Pending || StaffId == null)
                throw new DomainException("Chỉ có thể từ chối RFQ ở trạng thái chờ phân bổ");

            // // LƯU VẾT: Nhớ mặt ông nhân viên vừa từ chối
            // if (!_rejectedByStaffIds.Contains(StaffId))
            // {
            //     _rejectedByStaffIds.Add(StaffId);
            // }

            AddDomainEvent(new RFQRejectedEvent(Id, Code, StaffId));
            StaffId = null;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        // tất cả nhân viên từ chối
        public void MaskAsAllRejected()
        {
            if (Status != RFQStatus.Pending)
                throw new DomainException("Chỉ có thể xác nhận không có nhân viên nào xử lý khi ở trạng thái chờ phân bổ");

            Status = RFQStatus.Rejected;
            StaffId = null;
            UpdatedAt = DateTimeOffset.UtcNow;
            AddDomainEvent(new RFQAllRejectedEvent(Id, Code));
        }

        // Quản lý chỉ định nhân viên (ép gán)
        public void ForceAssign(StaffId staffId)
        {
            if (Status != RFQStatus.Rejected && Status != RFQStatus.Pending)
                throw new DomainException("Trạng thái RFQ không hợp lệ. Không thể chỉ định phân bổ.");

            StaffId = staffId;
            Status = RFQStatus.Accepted;
            UpdatedAt = DateTimeOffset.UtcNow;
            AddDomainEvent(new RFQForceAssignedEvent(Id, Code, StaffId));
        }

        // Đã báo giá lại khách hàng
        public void MarkAsResponded()
        {
            if (Status != RFQStatus.Accepted)
                throw new DomainException("Ghi nhận phản hồi báo giá không thành công.");

            Status = RFQStatus.Responded;
            UpdatedAt = DateTimeOffset.UtcNow;
            AddDomainEvent(new RFQMarkAsRespondedEvent(Id, Code));
        }

        // Báo giá được chốt thì coi là đã chuyển đổi thành đơn
        public void MarkAsConverted()
        {
            if (Status != RFQStatus.Responded)
                throw new DomainException("Ghi nhận chuyển đổi yêu cầu báo giá không thành công.");

            Status = RFQStatus.Converted;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void AddItem(
            ProductId productId,
            string productName,
            Quantity quantity,
            Code productCode,
            string manufacturer,
            string unit
        )
        {
            var item = new RFQItem(
                RFQItemId.New(),
                productId,
                productName,
                quantity,
                productCode,
                manufacturer,
                unit
            );
            _items.Add(item);
        }
    }
}