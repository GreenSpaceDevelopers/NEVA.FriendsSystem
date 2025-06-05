namespace Application.Dtos.Network;

public class DeserializedMessage
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public Guid ChatId { get; set; }
    public string? TextContent { get; set; }
    public string? FileContent { get; set; }
    public DateTime CreatedAt { get; set; }
}