using Application.Abstractions.Services.Auth;
using GS.IdentityServerApi;

namespace Application.Services.Auth;

public class TokenValidator(IdentityClient identityClient) : ITokenValidator
{
    public async Task<bool> ValidateToken(string? token)
    {
        var responseModel = await identityClient.GetUserSessionAsync(Guid.Parse(token!));
        
        return responseModel?.Success ?? false;
    }

    public async Task<string> GetUserIdFromToken(string? messageAccessToken)
    {
        var responseModel = await identityClient.GetUserSessionAsync(Guid.Parse(messageAccessToken!));
        
        return responseModel?.Data?.User.UserId.ToString() ?? string.Empty;
    }
}