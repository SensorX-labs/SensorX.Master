using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.WebApi.Configurations;

public class AuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        // Kiểm tra nếu bị từ chối truy cập (Forbidden) hoặc chưa xác thực (Challenged)
        if (authorizeResult.Forbidden || authorizeResult.Challenged)
        {
            context.Response.StatusCode = authorizeResult.Forbidden

                ? StatusCodes.Status403Forbidden

                : StatusCodes.Status401Unauthorized;

            string message = authorizeResult.Forbidden
                ? "Bạn không có quyền thực hiện hành động này (Forbidden)."

                : "Yêu cầu xác thực danh tính để truy cập (Unauthorized).";

            var result = Result.Failure(message);


            await context.Response.WriteAsJsonAsync(result);
            return;
        }

        // Nếu mọi thứ ổn, để mặc định xử lý tiếp
        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}
