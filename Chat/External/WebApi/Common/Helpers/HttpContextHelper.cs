using System.Runtime.CompilerServices;

namespace External.WebApi.Common.Helpers;

public static class HttpContextHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Guid GetUserId(this HttpContext httpContext) => Guid.NewGuid();
}