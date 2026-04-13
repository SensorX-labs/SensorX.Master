namespace SensorX.Master.Domain.Contexts.QuoteContext.ValueObjects
{
    public record CustomerInfo(
        string RecipientName,
        string RecipientPhone,
        string CompanyName,
        string Email,
        string Address,
        string TaxCode
    );
}
