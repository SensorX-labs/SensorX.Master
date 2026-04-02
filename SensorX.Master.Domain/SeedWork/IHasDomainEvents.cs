namespace SensorX.Master.Domain.SeedWork;

public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
    void AddDomainEvent(IDomainEvent eventItem);
    void RemoveDomainEvent(IDomainEvent eventItem);
}