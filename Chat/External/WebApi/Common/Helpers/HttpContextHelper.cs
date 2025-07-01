using System.Runtime.CompilerServices;
using GS.IdentityServerApi.Protocol.Models;

namespace WebApi.Common.Helpers;

public static class HttpContextHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid GetUserId(this HttpContext httpContext)
    {
        if (httpContext.Items["SessionContext"] is IdentitySession identitySession)
        {
            return identitySession.User.UserId;
        }

        return Guid.Empty;
    }
}