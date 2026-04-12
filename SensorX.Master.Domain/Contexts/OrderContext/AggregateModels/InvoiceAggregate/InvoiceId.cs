using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;

public record InvoiceId(Guid Value) : EntityId<InvoiceId>(Value);
