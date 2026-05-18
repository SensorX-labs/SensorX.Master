
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.Events;

public record OrderCreatedDomainEvent(
    Order Order,
    Guid OrderId,
    string OrderCode,
    string RecipientName,
    string RecipientPhone,
    string Address,
    string CompanyName,
    string TaxCode
) : IDomainEvent;
