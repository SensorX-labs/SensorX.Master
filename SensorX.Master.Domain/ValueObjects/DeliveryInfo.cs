using System.Text.Json.Serialization;
using SensorX.Master.Domain.Common.Exceptions;
namespace SensorX.Master.Domain.ValueObjects;

public record DeliveryInfo
{
    public string ReceiverName { get; init; }
    public Phone ReceiverPhone { get; init; }
    public string DeliveryAddress { get; init; }
    public string CompanyName { get; init; }
    public string TaxCode { get; init; }

    [JsonConstructor]
    public DeliveryInfo(string receiverName, Phone receiverPhone, string deliveryAddress, string companyName, string taxCode)
    {
        ReceiverName = receiverName;
        ReceiverPhone = receiverPhone;
        DeliveryAddress = deliveryAddress;
        CompanyName = companyName;
        TaxCode = taxCode;
    }

    public static DeliveryInfo Create(string receiverName, string receiverPhone, string deliveryAddress, string companyName, string taxCode)
    {
        if (string.IsNullOrWhiteSpace(receiverName)) throw new DomainException("ReceiverName cannot be empty.");
        if (string.IsNullOrWhiteSpace(deliveryAddress)) throw new DomainException("DeliveryAddress cannot be empty.");
        if (string.IsNullOrWhiteSpace(companyName)) throw new DomainException("CompanyName cannot be empty.");
        if (string.IsNullOrWhiteSpace(taxCode)) throw new DomainException("TaxCode cannot be empty.");

        return new DeliveryInfo(receiverName, Phone.Create(receiverPhone), deliveryAddress, companyName, taxCode);
    }
}