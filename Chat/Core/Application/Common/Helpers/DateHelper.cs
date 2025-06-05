using System.Runtime.CompilerServices;

namespace Application.Common.Helpers;

public static class DateHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static DateTime GetCurrentDateTime() => DateTime.UtcNow;
}