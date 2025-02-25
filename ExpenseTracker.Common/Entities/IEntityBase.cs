using MediatR;

namespace ExpenseTracker.Common.Entities
{
    public interface IEntityBase
    {
        public IReadOnlyList<INotification> DomainEvents { get; }
        public void AddDomainEvent(INotification eventItem);
        public void RemoveDomainEvent(INotification eventItem);
        public void ClearDomainEvents();
    }
}
