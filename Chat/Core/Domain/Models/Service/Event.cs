using Domain.Abstractions;

namespace Domain.Models.Service;

public class Event : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}