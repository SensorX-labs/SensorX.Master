namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate
{
    public class QuoteResponse
    {
        public QuoteResponseStatus ResponseType { get; private set; }
        public PaymentTerm PaymentTerm { get; private set; }
        public required string ShippingAddress { get; private set; }
        public string? Feedback { get; private set; }
    }
}
