using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.StrongIDs
{
    public record AccountId(Guid Value) : EntityId<AccountId>(Value);
}