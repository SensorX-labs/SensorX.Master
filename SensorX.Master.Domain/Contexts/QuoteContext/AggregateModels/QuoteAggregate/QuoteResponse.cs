namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate
{
    public sealed record QuoteResponse(
        QuoteResponseStatus ResponseType,
        PaymentTerm? PaymentTerm,
        string? ShippingAddress,
        string? RecipientName,
        string? RecipientPhone,
        string? Feedback
    );
}
