using SensorX.Master.Domain.Common.Exceptions;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.ValueObjects;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate
{
    public class Quote : Entity<QuoteId>, ICreationTrackable, IUpdateTrackable
    {
        private Quote() : base() { }

        public Quote(
            QuoteId id,
            QuoteId? parentId,
            Code code,
            RFQId rFQId,
            CustomerId customerId,
            CustomerInfo customerInfo,
            string? note,
            QuoteStatus status,
            QuoteResponse response,
            DateTimeOffset quoteDate,
            string reasonReject
        ) : base(id)
        {
            ParentId = parentId;
            Code = code;
            RFQId = rFQId;
            CustomerId = customerId;
            CustomerInfo = customerInfo;
            Note = note;
            Status = status;
            Response = response;
            QuoteDate = quoteDate;
            ReasonReject = reasonReject;
        }

        public QuoteId? ParentId { get; private set; }
        public Code Code { get; private set; }
        public RFQId RFQId { get; private set; }
        public CustomerId CustomerId { get; private set; }
        public CustomerInfo CustomerInfo { get; private set; }
        public string? Note { get; private set; }
        public QuoteStatus Status { get; private set; }
        public QuoteResponse Response { get; private set; }
        public DateTimeOffset QuoteDate { get; private set; }
        public string ReasonReject { get; private set; }

        private readonly List<QuoteItem> _quoteItems = [];
        public IReadOnlyList<QuoteItem> QuoteItems => _quoteItems.AsReadOnly();

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }

        public void AddItem(QuoteItem item)
        {
            _quoteItems.Add(item);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void RemoveItem(QuoteItemId quoteItemId)
        {
            _quoteItems.RemoveAll(item => item.Id == quoteItemId);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public Money GetSubtotal()
        {
            return _quoteItems.Select(item => item.GetLineAmount())
                        .Aggregate(Money.Zero(), (acc, next) => acc + next);
        }

        public Money GetTotalTax()
        {
            return _quoteItems.Select(item => item.GetTaxAmount())
                        .Aggregate(Money.Zero(), (acc, next) => acc + next);
        }

        public Money GetGrandTotal()
        {
            return _quoteItems.Select(item => item.GetTotalLineAmount())
                        .Aggregate(Money.Zero(), (acc, next) => acc + next);
        }

        public void RecordCustomerResponse(QuoteResponse response)
        {
            Response = response;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void SubmitForApproval()
        {
            if (Status is QuoteStatus.Draft or QuoteStatus.Returned)
            {
                Status = QuoteStatus.Pending;
                UpdatedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                throw new DomainException("Quote is not in a valid state to be submitted for approval.");
            }
        }

        public void WithDraw()
        {
            if (Status is QuoteStatus.Pending)
            {
                Status = QuoteStatus.Draft;
                UpdatedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                throw new DomainException("Quote is not in a valid state to be withdrawn.");
            }
        }

        public void Approve()
        {
            if (Status is QuoteStatus.Pending)
            {
                Status = QuoteStatus.Approved;
                UpdatedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                throw new DomainException("Quote is not in a valid state to be approved.");
            }
        }

        public void Reject(string reason)
        {
            if (Status is QuoteStatus.Pending)
            {
                Status = QuoteStatus.Returned;
                ReasonReject = reason;
                UpdatedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                throw new DomainException("Quote is not in a valid state to be rejected.");
            }
        }

        public void Publish()
        {
            if (Status is QuoteStatus.Approved)
            {
                Status = QuoteStatus.Sent;
                UpdatedAt = DateTimeOffset.UtcNow;
            }
            else
            {
                throw new DomainException("Quote is not in a valid state to be published.");
            }
        }
    }
}