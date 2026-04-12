namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate
{
    public class QuoteResponse
    {
        public QuoteResponseStatus ResponseType { get; set; }
        public PaymentTerm PaymentTerm { get; set; }
        public required string ShippingAddress { get; set; }
        public string? Feedback { get; set; }
    }
}
