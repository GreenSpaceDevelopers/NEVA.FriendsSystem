namespace Application.Dtos.Responses.BlackList;

public record UserSearchDto(
    Guid Id,
    string Username,
    string? AvatarUrl
); 