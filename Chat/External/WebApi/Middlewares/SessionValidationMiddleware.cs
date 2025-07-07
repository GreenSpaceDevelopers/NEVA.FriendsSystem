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
        var path = context.Request.Path.Value?.ToLower();
        if (path != null && path.Contains("/swagger"))
        {
            await next(context);
            return;
        }

        try
        {
            string? sessionId = null;

            var authHeader = context.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                sessionId = authHeader["Bearer ".Length..].Trim();
            }
            else if (context.Request.Query.TryGetValue("access_token", out var accessTokenValues))
            {
                sessionId = accessTokenValues.FirstOrDefault();
            }

            if (string.IsNullOrEmpty(sessionId))
            {
                throw new UnauthorizedException("Authorization header is required");
            }

            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                throw new UnauthorizedException("Invalid session ID format");
            }

            var responseModel = await identityClient.GetUserSessionAsync(sessionGuid);

            if (responseModel is not { Success: true })
            {
                if (responseModel?.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedException("Session not found or expired");
                }

                throw new Exception("Error on Identity side");
            }
            
            var identity = responseModel.Data;
            context.Items["SessionContext"] = identity;
            
            await identityClient.ProlongUserSession(sessionGuid);
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