using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Domain.ValueObjects;

public sealed record SenderInfo
(
    StaffId Id,
    string Name,
    Email Email,
    Phone? Phone
);