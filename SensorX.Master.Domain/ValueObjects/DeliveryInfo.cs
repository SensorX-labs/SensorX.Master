using SensorX.Master.Domain.Common.Exceptions;
namespace SensorX.Master.Domain.ValueObjects;

public record DeliveryInfo
{
    public string ReceiverName { get; init; }
    public Phone ReceiverPhone { get; init; }
    public string DeliveryAddress { get; init; }
    public string CompanyName { get; init; }
    public string TaxCode { get; init; }

    public DeliveryInfo(string receiverName, string receiverPhone, string deliveryAddress, string companyName, string taxCode)
    {
        if (string.IsNullOrWhiteSpace(receiverName)) throw new DomainException("ReceiverName cannot be empty.");
        if (string.IsNullOrWhiteSpace(deliveryAddress)) throw new DomainException("DeliveryAddress cannot be empty.");
        if (string.IsNullOrWhiteSpace(companyName)) throw new DomainException("CompanyName cannot be empty.");
        if (string.IsNullOrWhiteSpace(taxCode)) throw new DomainException("TaxCode cannot be empty.");

        ReceiverName = receiverName;
        ReceiverPhone = Phone.Create(receiverPhone);
        DeliveryAddress = deliveryAddress;
        CompanyName = companyName;
        TaxCode = taxCode;
    }
}