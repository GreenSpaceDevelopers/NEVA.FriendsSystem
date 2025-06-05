namespace Application.Abstractions.Services.Auth;

public interface ITokenValidator
{
    public bool ValidateToken(string? token);
    public string GetUserIdFromToken(string? messageAccessToken);
}