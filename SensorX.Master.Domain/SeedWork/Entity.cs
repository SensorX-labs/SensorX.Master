namespace SensorX.Master.Domain.SeedWork
{
    public abstract class Entity<TId> : IEquatable<Entity<TId>>, IHasDomainEvents where TId : VoId
    {
        protected Entity() { }
        protected Entity(TId id) => Id = id;

        private int? _requestedHashCode;
        public TId Id { get; init; } = null!;

        private readonly List<IDomainEvent> _domainEvents = [];
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents.Add(eventItem);
        }
        public void RemoveDomainEvent(IDomainEvent eventItem)
        {
            _domainEvents?.Remove(eventItem);
        }
        public void ClearDomainEvents()
        {
            _domainEvents?.Clear();
        }

        public override bool Equals(object? obj) => Equals(obj as Entity<TId>);
        public bool Equals(Entity<TId>? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.Id == other.Id && this.GetType() == other.GetType();
        }

        public override int GetHashCode()
        {
            _requestedHashCode ??= (Id.GetHashCode() ^ 31);
            return _requestedHashCode.Value;
        }

        public static bool operator ==(Entity<TId> left, Entity<TId> right) => Equals(left, right);
        public static bool operator !=(Entity<TId> left, Entity<TId> right) => !(left == right);
    }
}

