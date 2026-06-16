using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.WebApi.Extensions;

public static class ResultExtensions
{
    public static IResult ToResult<T>(this Result<T> result)
    {
        if (!result.IsSuccess)
        {
            if (result.Message != null && 
                (result.Message.Contains("chua duoc xac thuc", StringComparison.OrdinalIgnoreCase) || 
                 result.Message.Contains("chưa được xác thực", StringComparison.OrdinalIgnoreCase) || 
                 result.Message.Contains("not authenticated", StringComparison.OrdinalIgnoreCase) || 
                 result.Message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase)))
            {
                return Microsoft.AspNetCore.Http.Results.Json(result, statusCode: 401);
            }
            return TypedResults.BadRequest(result);
        }
        return TypedResults.Ok(result);
    }

    public static IResult ToResult(this Result result)
    {
        if (!result.IsSuccess)
        {
            if (result.Message != null && 
                (result.Message.Contains("chua duoc xac thuc", StringComparison.OrdinalIgnoreCase) || 
                 result.Message.Contains("chưa được xác thực", StringComparison.OrdinalIgnoreCase) || 
                 result.Message.Contains("not authenticated", StringComparison.OrdinalIgnoreCase) || 
                 result.Message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase)))
            {
                return Microsoft.AspNetCore.Http.Results.Json(result, statusCode: 401);
            }
            return TypedResults.BadRequest(result);
        }
        return TypedResults.Ok(result);
    }
}
