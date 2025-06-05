namespace Application.Abstractions.Services.ApplicationInfrastructure.Data;

public interface IEncryptionService
{
    public string Encrypt<T>(T data);
    public T Decrypt<T>(string data);
}