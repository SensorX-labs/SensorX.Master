using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.StrongIDs
{
    public record CustomerId(Guid Value) : EntityId<CustomerId>(Value);
}