namespace SensorX.Master.Domain.ValueObjects
{
    public record CustomerInfo(
        string RecipientName,
        Phone RecipientPhone,
        string ShippingAddress,
        string CompanyName,
        Email Email,
        string Address,
        string TaxCode
    );
}
