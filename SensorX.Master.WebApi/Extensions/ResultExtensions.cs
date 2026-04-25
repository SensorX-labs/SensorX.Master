using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.WebApi.Extensions;

public static class ResultExtensions
{
    public static IResult ToResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }

    public static IResult ToResult(this Result result)
    {
        return result.IsSuccess
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }
}
