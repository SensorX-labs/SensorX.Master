
using SensorX.Master.Domain.SeedWork;
namespace SensorX.Master.Domain.ValueObjects;

public record ProductId(Guid Value) : EntityId<ProductId>(Value);