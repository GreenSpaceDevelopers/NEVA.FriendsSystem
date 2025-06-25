using Domain.Models.Users;

namespace Application.Dtos.Responses.Profile;

public record ProfileDto(Guid Id, string Username, string? AvatarUrl, string? CoverUrl, PrivacySettingsEnums PrivacySetting);

public record ProfileValidationDto(bool IsAvailable, string? ErrorMessage); 