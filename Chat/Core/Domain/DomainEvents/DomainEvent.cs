using Domain.Abstractions;

namespace Domain.DomainEvents;

public class DomainEvent<T> where T : Entity<T>
{
    public T EntityAttached { get; set; } = null!;
    public EventType EventType { get; set; }
}

public enum EventType
{
    Created,
    Updated,
    Deleted
}