using Microsoft.AspNetCore.Authorization;
using SensorX.Master.Application.Common.Interfaces;

namespace SensorX.Master.WebApi.Configurations;

public class AuthorizeRoleAttribute : AuthorizeAttribute
{
    public AuthorizeRoleAttribute(params Role[] roles)
    {
        // Chuyển đổi mảng Enum thành chuỗi cách nhau bởi dấu phẩy
        // Ví dụ: [Role.Customer, Role.Admin] -> "Customer,Admin"
        Roles = string.Join(",", roles.Select(r => r.ToString()));
    }
}
