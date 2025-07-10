using System.Runtime.CompilerServices;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;

namespace Application.Services.ApplicationInfrastructure.Results;

public static class ResultsHelper
{
    public static IOperationResult Ok<TData>(TData data) => new OperationResult<TData>(data, true, 200);
    public static IOperationResult NotFound(string message) => new ErrorOperationResult(message, 404);
    public static IOperationResult Created(object id) => new OperationResult<object>(id, true, 201);
    public static IOperationResult BadRequest(string errorMessage) => new ErrorOperationResult(errorMessage, 400);
    public static IOperationResult NoContent() => new OperationResult<object>("No content", true, 204);
    public static IOperationResult Forbidden(string message) => new ErrorOperationResult(message, 403);
    public static IOperationResult Conflict(string message) => new ErrorOperationResult(message, 409);

    public static T GetValue<T>(this IOperationResult operationResult)
    {
        var data = operationResult.ObjectData;
        return Unsafe.As<object, T>(ref Unsafe.AsRef(ref data));
    }
}