using MassTransit;
using SensorX.Master.Domain.Common;

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
    StaffStatus Status,
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
    Department Department,
    StaffStatus Status
);

[MessageUrn("staff-avatar-updated")]
[EntityName("staff-avatar-updated")]
public sealed record UpdateStaffAvatarEvent(
    Guid Id,
    string AvatarUrl
);

[MessageUrn("staff-status-changed")]
[EntityName("staff-status-changed")]
public sealed record StaffStatusChangedEvent(
    Guid Id,
    Guid AccountId,
    StaffStatus Status
);