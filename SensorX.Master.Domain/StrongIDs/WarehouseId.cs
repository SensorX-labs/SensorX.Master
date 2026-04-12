
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.StrongIDs;

public record MasterId(Guid Value) : EntityId<MasterId>(Value)
{
    public static readonly MasterId Default = new(Guid.Parse("018e7b5a-1a5c-7b9e-8d4f-c3b2a1a0b9c8"));
}
