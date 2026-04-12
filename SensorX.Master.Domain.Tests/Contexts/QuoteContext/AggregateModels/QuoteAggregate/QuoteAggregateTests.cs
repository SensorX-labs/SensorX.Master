using SensorX.Master.Domain.Common.Exceptions;
using SensorX.Master.Domain.Contexts.QuoteContext;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.ValueObjects;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;
using Xunit;

namespace SensorX.Master.Domain.Tests.Contexts.QuoteContext.AggregateModels.QuoteAggregate
{
    /// <summary>
    /// Test suite for the Quote aggregate covering calculations, state transitions, and business rules.
    /// </summary>
    public class QuoteAggregateTests
    {
        /// <summary>
        /// Verifies that a Quote can be successfully initialized with all required fields.
        /// </summary>
        [Fact]
        public void CreateQuote_ShouldInitializeWithCorrectValues()
        {
            // Arrange
            var quoteId = new QuoteId(Guid.NewGuid());
            var code = Code.Create("BG");
            var rfqId = new RFQId(Guid.NewGuid());
            var customerId = new CustomerId(Guid.NewGuid());
            var customerInfo = GetTestCustomerInfo();
            var response = new QuoteResponse
            {

                ResponseType = QuoteResponseStatus.Accept, // Fixed
                ShippingAddress = "Hanoi, Vietnam"
            };

            // Act
            var quote = new Quote(
                quoteId,
                code,
                rfqId,
                customerId,
                customerInfo,
                "Test Note",
                QuoteStatus.Draft,
                response,
                DateTimeOffset.UtcNow,
                ""
            );

            // Assert
            Assert.Equal(quoteId, quote.Id);
            Assert.Equal(QuoteStatus.Draft, quote.Status);
            Assert.Empty(quote.LineItems);
        }

        /// <summary>
        /// Ensures that adding an item to the quote correctly updates the subtotal, tax, and grand total.
        /// </summary>
        [Fact]
        public void AddItem_ShouldUpdateTotalsAndUpdatedAt()
        {
            // Arrange
            var quote = CreateSut();
            var item = new QuoteItem(
                new QuoteItemId(Guid.NewGuid()),
                new ProductId(Guid.NewGuid()),
                Code.Create("PRD"),
                "Mfr",
                "Unit",
                new Quantity(10),
                Money.FromVnd(100000),
                Percent.From(10)
            );

            // Act
            quote.AddItem(item);

            // Assert
            Assert.Single(quote.LineItems);
            Assert.Equal(Money.FromVnd(1000000), quote.GetSubtotal());
            Assert.Equal(Money.FromVnd(100000), quote.GetTotalTax());
            Assert.Equal(Money.FromVnd(1100000), quote.GetGrandTotal());
            Assert.NotNull(quote.UpdatedAt);
        }

        /// <summary>
        /// Verifies that a Quote in Draft status can transition to Pending status.
        /// </summary>
        [Fact]
        public void SubmitForApproval_FromDraft_ShouldChangeStatusToPending()
        {
            // Arrange
            var quote = CreateSut();

            // Act

            quote.SubmitForApproval();

            // Assert
            Assert.Equal(QuoteStatus.Pending, quote.Status);
        }

        /// <summary>
        /// Ensures that an InvalidOperationException (via DomainException) is thrown when trying to submit an already approved quote.
        /// </summary>
        [Fact]
        public void SubmitForApproval_FromApproved_ShouldThrowDomainException()
        {
            // Arrange
            var quote = CreateSut();
            quote.SubmitForApproval(); // -> Pending
            quote.Approve();           // -> Approved

            // Act & Assert
            var ex = Assert.Throws<DomainException>(() => quote.SubmitForApproval());
            Assert.Contains("valid state", ex.Message);
        }

        /// <summary>
        /// Verifies that a Pending quote can be Approved.
        /// </summary>
        [Fact]
        public void Approve_FromPending_ShouldChangeStatusToApproved()
        {
            // Arrange
            var quote = CreateSut();
            quote.SubmitForApproval();

            // Act
            quote.Approve();

            // Assert
            Assert.Equal(QuoteStatus.Approved, quote.Status);
        }

        /// <summary>
        /// Verifies that a Pending quote can be Rejected/Returned with a specific reason.
        /// </summary>
        [Fact]
        public void Reject_FromPending_ShouldChangeStatusToReturned()
        {
            // Arrange
            var quote = CreateSut();
            quote.SubmitForApproval();

            // Act
            quote.Reject("Prices are too high");

            // Assert
            Assert.Equal(QuoteStatus.Returned, quote.Status);
            Assert.Equal("Prices are too high", quote.ReasonReject);
        }

        /// <summary>
        /// Verifies that an Approved quote can be Published (Sent to customer).
        /// </summary>
        [Fact]
        public void Publish_FromApproved_ShouldChangeStatusToSent()
        {
            // Arrange
            var quote = CreateSut();
            quote.SubmitForApproval();
            quote.Approve();

            // Act
            quote.Publish();

            // Assert
            Assert.Equal(QuoteStatus.Sent, quote.Status);
        }

        /// <summary>
        /// Helper method to create a System Under Test (Sut) with default valid values.
        /// </summary>
        private Quote CreateSut()
        {
            return new Quote(
                new QuoteId(Guid.NewGuid()),
                Code.Create("BG"),
                new RFQId(Guid.NewGuid()),
                new CustomerId(Guid.NewGuid()),
                GetTestCustomerInfo(),
                "Note",
                QuoteStatus.Draft,
                new QuoteResponse { ShippingAddress = "Addr" },
                DateTimeOffset.UtcNow,
                ""
            );
        }

        /// <summary>
        /// Helper method to generate dummy customer information for testing purposes.
        /// </summary>
        private CustomerInfo GetTestCustomerInfo()
        {
            return new CustomerInfo(
                "Recipient Name",
                "0123456789",
                "Company XYZ",
                "test@test.com",
                "123 Street",
                "TAX123"
            );
        }
    }
}
