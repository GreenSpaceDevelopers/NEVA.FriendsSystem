using Domain.Abstractions;

namespace Domain.Models.Messaging;

public class Attachment : IEntity
{
    public Guid Id { get; set; }
    public Guid TypeId { get; set; }
    public string Url { get; set; } = string.Empty;
    public AttachmentType Type { get; set; } = null!;
}

public class AttachmentType
{
    public Guid Id { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
}

public enum AttachmentTypes
{
    Image,
    Video,
    Audio,
    File,
    Sticker,
}