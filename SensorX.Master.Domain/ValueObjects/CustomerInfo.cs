namespace SensorX.Master.Domain.ValueObjects
{
    public record CustomerInfo(
        string CompanyName,
        Email Email,
        string Address,
        string TaxCode,
        Phone Phone
    );
}
