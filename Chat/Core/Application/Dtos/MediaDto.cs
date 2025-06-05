namespace Application.Dtos;

public class MediaDto
{
    public string Url { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
}