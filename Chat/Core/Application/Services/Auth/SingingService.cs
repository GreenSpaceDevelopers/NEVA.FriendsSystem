using System.Text;
using System.Text.Json;
using Application.Abstractions.Services.Auth;
using Infrastructure.Common.Helpers;

namespace Application.Services.Auth;

public class SingingService : ISigningService
{
    public string Sign<T>(T data)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
        var hmac = HMAC.ComputeHMACHash(bytes, "secretKey");

        return Convert.ToBase64String(hmac);
    }

    public bool Verify<T>(T data, string signature)
    {
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));
        var hmac = HMAC.ComputeHMACHash(bytes, "secretKey");
        return HMAC.VerifyHMAC(bytes, hmac, "secretKey");
    }
}