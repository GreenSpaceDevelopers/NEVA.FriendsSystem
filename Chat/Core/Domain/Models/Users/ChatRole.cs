using Domain.Abstractions;

namespace Domain.Models;

public class ChatRole : IEntity
{
    public Guid Id { get; set; }
}