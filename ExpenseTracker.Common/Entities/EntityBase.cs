using MediatR;

namespace ExpenseTracker.Common.Entities
{
    public abstract class EntityBase<T> : IEntityBase
    {
        private readonly List<INotification> _domainEvents = new();
        private readonly object _lock = new();

        public IReadOnlyList<INotification> DomainEvents => _domainEvents.AsReadOnly();

        public T Id { get; set; }

        protected EntityBase() { }

        public void AddDomainEvent(INotification eventItem)
        {
            lock (_lock)
            {
                _domainEvents.Add(eventItem);
            }
        }

        public void RemoveDomainEvent(INotification eventItem)
        {
            lock (_lock)
            {
                _domainEvents.Remove(eventItem);
            }
        }

        public void ClearDomainEvents()
        {
            lock (_lock)
            {
                _domainEvents.Clear();
            }
        }

        public bool Equals(EntityBase<T> other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            if (this.IsTransient() || other.IsTransient())
                return false;

            return this.Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EntityBase<T>);
        }

        public override int GetHashCode()
        {
            if (!IsTransient())
            {
                return this.Id.GetHashCode() ^ 31;
            }
            else
            {
                return base.GetHashCode();
            }
        }

        public bool IsTransient() => EqualityComparer<T>.Default.Equals(Id, default(T));

        public static bool operator ==(EntityBase<T> left, EntityBase<T> right)
        {
            if (Equals(left, null))
            {
                return (Equals(right, null));
            }
            else
            {
                return left.Equals(right);
            }
        }

        public static bool operator !=(EntityBase<T> left, EntityBase<T> right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return $"{GetType().Name} [Id={Id}]";
        }
    }

    public abstract class EntityBase : EntityBase<Guid>
    {
    }
}
