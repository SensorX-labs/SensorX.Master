using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Common.ReadModel
{
    public class Customer : IAggregateRoot
    {
        public Customer(
            CustomerId id,
            AccountId accountId,
            string companyName,
            string taxCode,
            Email email,
            Phone? phone,
            string? address,
            DateTimeOffset createdAt
        )
        {
            Id = id;
            AccountId = accountId;
            CompanyName = companyName;
            TaxCode = taxCode;
            Email = email;
            Phone = phone;
            Address = address;
            CreatedAt = createdAt;
        }

        public CustomerId Id { get; }
        public AccountId AccountId { get; }
        public string CompanyName { get; private set; }
        public string TaxCode { get; private set; }
        public Email Email { get; private set; }
        public Phone? Phone { get; private set; }
        public string? Address { get; private set; }

        public Phone? RecipientPhone { get; private set; }
        public string? RecipientName { get; private set; }
        public string? ShippingAddress { get; private set; }

        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        public void Update(
            string companyName,
            Email email,
            Phone? phone,
            string? address,
            string taxCode,
            DateTimeOffset? updatedAt
        )
        {
            CompanyName = companyName;
            Email = email;
            Phone = phone;
            Address = address;
            TaxCode = taxCode;
            UpdatedAt = updatedAt;
        }

        public void UpdateShippingInfo(
            Phone? recipientPhone,
            string? recipientName,
            string? shippingAddress,
            DateTimeOffset? updatedAt
        )
        {
            RecipientPhone = recipientPhone;
            RecipientName = recipientName;
            ShippingAddress = shippingAddress;
            UpdatedAt = updatedAt;
        }
    }
}