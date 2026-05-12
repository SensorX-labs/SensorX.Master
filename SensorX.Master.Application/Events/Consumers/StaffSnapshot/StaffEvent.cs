using MassTransit;

namespace SensorX.Master.Application.Events.Consumers.StaffSnapshot;

public enum Department
{
    Sale, // Phòng ban kinh doanh
    Warehouse, // Phòng ban kho
    Manager, // Phòng ban quản lý
}

[MessageUrn("staff-created")]
[EntityName("staff-created")]
public sealed record CreateStaffEvent(
    Guid Id,
    Guid AccountId,
    string Code,
    string Name,
    string Email,
    Department Department,
    DateTimeOffset CreatedAt
);

[MessageUrn("staff-updated")]
[EntityName("staff-updated")]
public sealed record UpdateStaffEvent(
    Guid Id,
    string Name,
    string? Phone,
    string Email,
    string? CitizenId,
    string? Biography,
    DateTimeOffset JoinDate,
    Department Department
);