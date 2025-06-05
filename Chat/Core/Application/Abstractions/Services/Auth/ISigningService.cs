namespace Application.Abstractions.Services.Auth;

public interface ISigningService
{
    public string Sign<T>(T data);
    public bool Verify<T>(T data, string signature);
}