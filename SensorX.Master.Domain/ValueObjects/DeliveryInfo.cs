using System.Text.Json.Serialization;
using SensorX.Master.Domain.Common.Exceptions;
namespace SensorX.Master.Domain.ValueObjects;

public record DeliveryInfo
{
    public string RecipientName { get; init; }
    public Phone RecipientPhone { get; init; }
    public string CompanyName { get; init; }
    public Email Email { get; init; }
    public string ShippingAddress { get; init; }
    public string TaxCode { get; init; }

    [JsonConstructor]
    public DeliveryInfo(string recipientName, Phone recipientPhone, string companyName, Email email, string shippingAddress, string taxCode)
    {
        RecipientName = recipientName;
        RecipientPhone = recipientPhone;
        CompanyName = companyName;
        Email = email;
        ShippingAddress = shippingAddress;
        TaxCode = taxCode;
    }

    public static DeliveryInfo Create(string recipientName, string recipientPhone, string shippingAddress, string companyName, Email email, string taxCode)
    {
        if (string.IsNullOrWhiteSpace(recipientName)) throw new DomainException("RecipientName cannot be empty.");
        if (string.IsNullOrWhiteSpace(shippingAddress)) throw new DomainException("ShippingAddress cannot be empty.");
        if (string.IsNullOrWhiteSpace(companyName)) throw new DomainException("CompanyName cannot be empty.");
        if (string.IsNullOrWhiteSpace(taxCode)) throw new DomainException("TaxCode cannot be empty.");

        return new DeliveryInfo(recipientName, Phone.Create(recipientPhone), companyName, email, shippingAddress, taxCode);
    }
}