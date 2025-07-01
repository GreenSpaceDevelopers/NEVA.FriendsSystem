using System.Net;
using GS.IdentityServerApi;

namespace WebApi.Middlewares;

public class SessionValidationMiddleware(
    ILogger<SessionValidationMiddleware> logger,
    RequestDelegate next,
    IdentityClient identityClient)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var authHeader = context.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var sessionId = authHeader["Bearer ".Length..].Trim();
                if (Guid.TryParse(sessionId, out var sessionGuid))
                {
                    var responseModel = await identityClient.GetUserSessionAsync(sessionGuid);

                    if (responseModel is not { Success: true })
                    {
                        if (responseModel?.StatusCode == HttpStatusCode.Forbidden || responseModel?.StatusCode == HttpStatusCode.Unauthorized)
                        {
                            throw new UnauthorizedException("Session not found or expired");
                        }

                        throw new Exception("Error on Identity side");
                    }
                    
                    var identity = responseModel.Data;
                    context.Items["SessionContext"] = identity;
                    
                    await identityClient.ProlongUserSession(sessionGuid);
                }
            }
        }
        catch (Exception e)
        {
            logger.LogError("SessionValidationMiddleware error: {Message}", e.Message);
            logger.LogError("SessionValidationMiddleware error: {StackTrace}", e.StackTrace);
            throw;
        }

        await next(context);
    }
}

public class UnauthorizedException(string message) : Exception(message);