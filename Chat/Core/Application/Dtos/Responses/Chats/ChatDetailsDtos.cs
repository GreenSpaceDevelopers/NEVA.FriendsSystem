using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Chats;

public record ChatParticipantDto(
    Guid Id,
    string Username,
    string? AvatarUrl);

[SwaggerSchema(Description = "Детальная информация о чате")]
public record ChatDetailsDto(
    Guid Id,
    string Name,
    string? ImageUrl,
    bool IsGroup,
    IReadOnlyCollection<ChatParticipantDto> Participants,
    LastChatMessagePreview LastMessagePreview); 