namespace SensorX.Master.Domain.ValueObjects
{
    public record CustomerInfo(
        string RecipientName,
        string RecipientPhone,
        string CompanyName,
        Email Email,
        string Address,
        string TaxCode
    );
}
