using Application.Abstractions.Services.ApplicationInfrastructure.Results;

namespace WebApi.Common.Helpers;

public static class ApiResultExtensions
{
    public static IResult ToApiResult(this IOperationResult result)
    {
        return result.StatusCode switch
        {
            200 => Results.Ok(result.ObjectData),
            201 => Results.Created(string.Empty, result.ObjectData),
            204 => Results.NoContent(),
            400 => Results.BadRequest(result.ObjectData),
            403 => Results.StatusCode(403),
            404 => Results.NotFound(result.ObjectData),
            _ => Results.StatusCode(result.StatusCode)
        };
    }
}