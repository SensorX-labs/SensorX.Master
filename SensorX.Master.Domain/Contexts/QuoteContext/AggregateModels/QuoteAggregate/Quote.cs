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

        public Code Code { get; private set; }
        public RFQId RFQId { get; private set; }
        public CustomerId CustomerId { get; private set; }
        public CustomerInfo CustomerInfo { get; private set; }
        public string? Note { get; private set; }
        public QuoteStatus Status { get; private set; }
        public QuoteResponse Response { get; private set; }
        public DateTimeOffset QuoteDate { get; private set; }
        public string ReasonReject { get; private set; }

        private readonly List<QuoteItem> _lineItems = [];
        public IReadOnlyList<QuoteItem> LineItems => _lineItems.AsReadOnly();

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }

        /// <summary>
        /// Adds a new item to the quote and updates the last modified timestamp.
        /// </summary>
        public void AddItem(QuoteItem item)
        {
            _lineItems.Add(item);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Removes an item from the quote based on its ID and updates the last modified timestamp.
        /// </summary>
        public void RemoveItem(QuoteItemId quoteItemId)
        {
            _lineItems.RemoveAll(item => item.Id == quoteItemId);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Calculates the subtotal of all items in the quote (without tax).
        /// </summary>
        public Money GetSubtotal()
        {
            return _lineItems.Select(item => item.GetLineAmount())
                        .Aggregate(Money.Zero(), (acc, next) => acc + next);
        }

        /// <summary>
        /// Calculates the total tax amount for all items in the quote.
        /// </summary>
        public Money GetTotalTax()
        {
            return _lineItems.Select(item => item.GetTaxAmount())
                        .Aggregate(Money.Zero(), (acc, next) => acc + next);
        }

        /// <summary>
        /// Calculates the total line amount (subtotal + tax) for the entire quote.
        /// </summary>
        public Money GetGrandTotal()
        {
            return _lineItems.Select(item => item.GetTotalLineAmount())
                        .Aggregate(Money.Zero(), (acc, next) => acc + next);
        }

        /// <summary>
        /// Records the customer's response (Accept/Declined) to the quote.
        /// </summary>
        public void RecordCustomerResponse(QuoteResponse response)
        {
            Response = response;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Transitions the quote status to Pending for approval.
        /// Valid only for Draft or Returned quotes.
        /// </summary>
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

        /// <summary>
        /// Withdraws a pending quote back to Draft state.
        /// </summary>
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

        /// <summary>
        /// Approves a pending quote.
        /// </summary>
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

        /// <summary>
        /// Rejects a pending quote and records the reason.
        /// </summary>
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

        /// <summary>
        /// Publishes an approved quote, sending it to the customer (Status Sent).
        /// </summary>
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