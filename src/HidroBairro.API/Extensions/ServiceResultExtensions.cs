using HidroBairro.Application.Common;

namespace HidroBairro.API.Extensions;

public static class ServiceResultExtensions
{
    public static IResult ToHttpResult(this ServiceResult result)
    {
        if (result.Succeeded)
            return Results.NoContent();

        return ToErrorResult(result);
    }

    public static IResult ToHttpResult<T>(this ServiceResult<T> result, string? createdLocation = null)
    {
        if (result.Succeeded)
        {
            if (result.Created)
            {
                return createdLocation is null
                    ? Results.Created((string?)null, result.Data)
                    : Results.Created(createdLocation, result.Data);
            }

            return Results.Ok(result.Data);
        }

        return ToErrorResult(result);
    }

    private static IResult ToErrorResult(ServiceResult result)
    {
        var body = new { error = result.Error };

        if (result.NotFound)
            return Results.NotFound(body);

        if (result.Forbidden)
            return Results.Json(body, statusCode: StatusCodes.Status403Forbidden);

        if (result.Conflict)
            return Results.Conflict(body);

        return Results.BadRequest(body);
    }
}
