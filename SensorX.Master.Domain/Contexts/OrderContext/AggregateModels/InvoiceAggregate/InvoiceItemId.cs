using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;

public record InvoiceItemId(Guid Value) : EntityId<InvoiceItemId>(Value);