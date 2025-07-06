namespace Application.Abstractions.Services.Auth;

public interface ITokenValidator
{
    public Task<bool> ValidateToken(string? token);
    public Task<string> GetUserIdFromToken(string? messageAccessToken);
}