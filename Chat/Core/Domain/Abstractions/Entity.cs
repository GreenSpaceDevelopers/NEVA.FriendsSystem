using System.ComponentModel.DataAnnotations.Schema;
using Domain.DomainEvents;

namespace Domain.Abstractions;

public abstract class Entity<T> : IEntity where T : Entity<T>
{
    public Guid Id { get; set; }
    [NotMapped] public DomainEvent<T> DomainEvents { get; set; } = null!;
}

public interface IEntity
{
    public Guid Id { get; set; }
}