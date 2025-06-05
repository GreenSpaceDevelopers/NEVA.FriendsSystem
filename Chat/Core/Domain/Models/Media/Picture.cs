using Domain.Abstractions;

namespace Domain.Models.Media;

public class Picture : IEntity
{
    public Guid Id { get; set; }
    public string Url { get; set; } = string.Empty;
}