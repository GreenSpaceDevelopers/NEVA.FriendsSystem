using GS.IdentityServerApi;

namespace WebApi.Middlewares;

public class SessionValidationMiddleware(
    RequestDelegate next,
    IdentityClient identityClient)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var sessionId = authHeader["Bearer ".Length..].Trim();
            if (Guid.TryParse(sessionId, out var sessionGuid))
            {
                var responseModel = await identityClient.GetUserSessionAsync(sessionGuid);
                if (responseModel is { Success: true, Data: not null })
                {
                    var identity = responseModel.Data;
                    context.Items["SessionContext"] = identity;
                    
                    await identityClient.ProlongUserSession(sessionGuid);
                }
            }
        }

        await next(context);
    }
} 