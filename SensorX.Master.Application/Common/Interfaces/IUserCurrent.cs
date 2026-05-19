namespace SensorX.Master.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid? UserId { get; }
    Role? Role { get; }
    bool IsAuthenticated { get; }
}

public enum Role
{
    Customer,
    WarehouseStaff,
    SaleStaff,
    Manager,
    Admin
}

