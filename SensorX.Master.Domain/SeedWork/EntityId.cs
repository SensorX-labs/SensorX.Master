namespace SensorX.Master.Domain.SeedWork;

public abstract record EntityId<TId> : VoId
    where TId : EntityId<TId>
{
    protected EntityId(Guid value) : base(value) { }
    public static TId New() => (TId)Activator.CreateInstance(typeof(TId), Guid.CreateVersion7())!;
}