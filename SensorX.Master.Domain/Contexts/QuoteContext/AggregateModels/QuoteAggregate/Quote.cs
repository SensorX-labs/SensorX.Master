using SensorX.Master.Domain.Common.Exceptions;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.Events;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate
{
    public class Quote : Entity<QuoteId>, IAggregateRoot, ICreationTrackable, IUpdateTrackable
    {
#pragma warning disable CS8618 // EF Core requires parameterless constructor
        private Quote() : base() { }
#pragma warning restore CS8618

        private Quote(
            QuoteId id,
            Code code,
            RFQId rFQId,
            CustomerId customerId,
            SenderInfo sender,
            CustomerInfo customerInfo,
            QuoteStatus status,
            string reasonReject
        ) : base(id)
        {
            Code = code;
            RFQId = rFQId;
            CustomerId = customerId;
            SenderInfo = sender;
            CustomerInfo = customerInfo;
            Status = status;
            ReasonReject = reasonReject;
            AddDomainEvent(new QuoteCreatedEvent(Id, rFQId));
        }

        public static Quote CreateDraft(
            RFQId rFQId,
            CustomerId customerId,
            SenderInfo sender,
            CustomerInfo customerInfo
        )
        {
            return new Quote(
                QuoteId.New(),
                Code.Create("QTE"),
                rFQId,
                customerId,
                sender,
                customerInfo,
                QuoteStatus.Draft,
                string.Empty
            );
        }

        public Code Code { get; private set; }
        public RFQId RFQId { get; private set; }
        public CustomerId CustomerId { get; private set; }
        public CustomerInfo CustomerInfo { get; private set; }
        public string? Note { get; private set; }
        public QuoteStatus Status { get; private set; }
        public SenderInfo SenderInfo { get; private set; }
        public QuoteResponse? Response { get; private set; }
        public DateTimeOffset? QuoteDate { get; private set; }
        public string ReasonReject { get; private set; }

        private readonly List<QuoteItem> _lineItems = [];
        public IReadOnlyList<QuoteItem> LineItems => _lineItems.AsReadOnly();

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

        public void SetNote(string note)
        {
            Note = note;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        public void SetSenderInfo(SenderInfo sender)
        {
            SenderInfo = sender;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Adds a new item to the quote and updates the last modified timestamp.
        /// </summary>
        public void AddItem(
            ProductId productId,
            Code productCode,
            string manufacturer,
            string unit,
            Quantity quantity,
            Money unitPrice,
            Percent taxRate
        )
        {
            var existingItem = _lineItems.FirstOrDefault(x => x.ProductId == productId);
            if (existingItem is not null)
            {
                existingItem.Update(quantity, unitPrice, taxRate);
                return;
            }

            var quoteItem = new QuoteItem(
                QuoteItemId.New(),
                productId,
                productCode,
                manufacturer,
                unit,
                quantity,
                unitPrice,
                taxRate
            );
            _lineItems.Add(quoteItem);
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        // <summary>
        // Removes an item from the quote based on its product ID and updates the last modified timestamp.
        // </summary>
        public void RemoveItem(ProductId productId)
        {
            var quoteItem = _lineItems.FirstOrDefault(item => item.ProductId == productId);
            if (quoteItem is not null)
            {
                _lineItems.Remove(quoteItem);
                UpdatedAt = DateTimeOffset.UtcNow;
            }
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
            if (Response is not null)
                throw new DomainException("Customer has already responded to the quote.");

            if (Status is QuoteStatus.Sent)
            {
                Response = response;
                UpdatedAt = DateTimeOffset.UtcNow;
                AddDomainEvent(new CustomerRespondedQuoteEvent(Id, RFQId, response));
            }
            else
            {
                throw new DomainException("Quote is not in a valid state to be accepted.");
            }
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
                QuoteDate = DateTimeOffset.UtcNow;
                UpdatedAt = DateTimeOffset.UtcNow;
                AddDomainEvent(new PublishQuoteEvent(Id, RFQId));
            }
            else
            {
                throw new DomainException("Quote is not in a valid state to be published.");
            }
        }

        /// <summary>
        /// Staff Cancelled quote or when staff publish 1 quote then any quote have rfqId Cancelled
        /// </summary>
        public void Cancel()
        {
            Status = QuoteStatus.Cancelled;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Transitions the quote status to Ordered when an order is created from it.
        /// </summary>
        public void MarkAsOrdered()
        {
            if (Status != QuoteStatus.Sent)
                throw new DomainException("Chỉ có báo giá đã gửi mới có thể chuyển sang trạng thái đã sinh đơn.");

            Status = QuoteStatus.Ordered;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
