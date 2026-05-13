using System.Security.Claims;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.Interfaces;

namespace SensorX.Master.WebApi.Middleware;

public class UserContextMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext context, ILogger<UserContextMiddleware> logger)
    {
        var userIdHeader = context.Request.Headers["X-User-Id"].FirstOrDefault();
        var userRolesHeader = context.Request.Headers["X-User-Roles"].FirstOrDefault();

        if (!string.IsNullOrEmpty(userIdHeader))
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userIdHeader),
            };

            if (!string.IsNullOrEmpty(userRolesHeader))
            {
                // Tách và chuẩn hóa Role dựa trên Enum Role của hệ thống
                var roles = userRolesHeader.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var role in roles)
                {
                    if (Enum.TryParse<Role>(role.Trim(), true, out var roleEnum))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roleEnum.ToString()));
                    }
                    else
                    {
                        logger.LogWarning("⚠️ UserContextMiddleware: Role lạ từ Gateway bị bỏ qua: {Role}", role);
                    }
                }
            }

            // Log định dạng mới dễ nhìn hơn
            logger.LogInformation(
                "\n┌────── [CURRENT USER] ──────┐" +
                "\n│ 👤 ID:    {UserId}" +
                "\n│ 🔑 Roles: {Roles}" +
                "\n│ 📍 Path:  {Method} {Path}" +
                "\n└───────────────────────────┘",
                userIdHeader,
                userRolesHeader ?? "N/A",
                context.Request.Method,
                context.Request.Path);

            var identity = new ClaimsIdentity(claims, "Gateway");
            context.User = new ClaimsPrincipal(identity);
        }
        else
        {
            logger.LogWarning("⚠️ UserContextMiddleware: No X-User-Id header found for {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }

        await _next(context);
    }
}