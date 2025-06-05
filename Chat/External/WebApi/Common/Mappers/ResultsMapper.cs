

using Application.Abstractions.Services.ApplicationInfrastructure.Results;

namespace WebApi.Common.Mappers;

public static class ResultsMapper
{
    public static IResult ToResult(this IOperationResult operationResult)
    {
        return Results.Json(operationResult.ObjectData, statusCode: operationResult.StatusCode);
    }
}
