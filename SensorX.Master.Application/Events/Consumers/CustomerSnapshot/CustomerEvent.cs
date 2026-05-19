using MassTransit;

namespace SensorX.Master.Application.Events.Consumers.CustomerSnapshot;

[MessageUrn("customer-created")]
[EntityName("customer-created")]
public sealed record CreateCustomerEvent(
    Guid Id,
    Guid AccountId,
    string CompanyName,
    string TaxCode,
    string Email,
    string? Phone,
    string? Address,
    DateTimeOffset CreatedAt
);

[MessageUrn("Customer-Updated-Info-Event")]
[EntityName("Customer-Updated-Info-Event")]
public sealed record UpdateCustomerInfoEvent(
    Guid Id,
    string Name,
    string? Phone,
    string Email,
    string TaxCode,
    string? Address,
    DateTimeOffset? UpdatedAt
);

[MessageUrn("customer-shipping-updated")]
[EntityName("customer-shipping-updated")]
public sealed record UpdateShippingInfoEvent(
    Guid Id,
    string? ShippingAddress,
    string? ReceiverName,
    string? ReceiverPhone,
    DateTimeOffset? UpdatedAt
);