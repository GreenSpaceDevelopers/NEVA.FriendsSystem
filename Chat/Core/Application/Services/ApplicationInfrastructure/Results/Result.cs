namespace Application.Abstractions.Services.ApplicationInfrastructure.Results;

public interface IOperationResult
{
    public bool IsSuccess { get; }
    public int StatusCode { get; }
    public object ObjectData { get; }
}

public record OperationResult<T>(T Data, bool IsSuccess, int StatusCode) : IOperationResult
{
    public object ObjectData { get; } = Data!;
}

public record ErrorOperationResult(string message, int statusCode) : OperationResult<string>(message, false, statusCode);