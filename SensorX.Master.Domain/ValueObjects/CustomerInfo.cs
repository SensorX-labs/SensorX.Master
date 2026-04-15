namespace SensorX.Master.Domain.ValueObjects
{
    public record CustomerInfo(
        string RecipientName,
        Phone RecipientPhone,
        string CompanyName,
        Email Email,
        string Address,
        string TaxCode
    );
}
