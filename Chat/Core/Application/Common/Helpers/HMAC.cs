using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Common.Helpers;

public static class HMAC
{
    public static byte[] ComputeHMACHash(byte[] data, string secretKey)
    {
        using var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
        return hmacsha256.ComputeHash(data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool VerifyHMAC(byte[] data, byte[] hmac, string secretKey)
    {
        return hmac.SequenceEqual(ComputeHMACHash(data, secretKey));
    }
}