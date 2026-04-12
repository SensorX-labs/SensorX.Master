using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;

public record PaymentId(Guid Value) : EntityId<PaymentId>(Value);
