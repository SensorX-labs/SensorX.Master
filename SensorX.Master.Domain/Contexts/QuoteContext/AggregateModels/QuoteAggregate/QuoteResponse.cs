namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate
{
    public class QuoteResponse
    {
        public QuoteResponseStatus ResponseType { get; init; }
        public PaymentTerm PaymentTerm { get; init; }
        public required string ShippingAddress { get; init; }
        public string? Feedback { get; init; }
    }
}
