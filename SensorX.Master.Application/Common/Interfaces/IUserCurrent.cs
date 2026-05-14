namespace SensorX.Master.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    Role? Role { get; }
    bool IsAuthenticated { get; }
    List<string>? Roles { get; }
}

public enum Role
{
    Customer,
    WarehouseStaff,
    SaleStaff,
    Manager,
    Admin
}

