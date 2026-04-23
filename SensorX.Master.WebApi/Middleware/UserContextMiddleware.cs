using System.Security.Claims;

namespace SensorX.Master.WebApi.Middleware;

public class UserContextMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var userIdHeader = context.Request.Headers["X-User-Id"].FirstOrDefault();
        var userRolesHeader = context.Request.Headers["X-User-Roles"].FirstOrDefault();

        if (!string.IsNullOrEmpty(userIdHeader))
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userIdHeader)
            };

            if (!string.IsNullOrEmpty(userRolesHeader))
            {
                claims.Add(new Claim(ClaimTypes.Role, userRolesHeader));
            }

            var identity = new ClaimsIdentity(claims, "Gateway");
            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context);
    }
}

public static class UserContextMiddlewareExtensions
{
    public static IApplicationBuilder UseUserContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<UserContextMiddleware>();
    }
}
