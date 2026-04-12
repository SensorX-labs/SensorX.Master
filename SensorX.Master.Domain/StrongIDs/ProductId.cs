
using SensorX.Master.Domain.SeedWork;
namespace SensorX.Master.Domain.StrongIDs;

public record ProductId(Guid Value) : EntityId<ProductId>(Value);
