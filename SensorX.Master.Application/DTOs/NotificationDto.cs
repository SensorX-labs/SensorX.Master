using System;

namespace SensorX.Master.Application.DTOs;

public record NotificationDto(
    Guid Id,
    string Title,
    string Content,
    string Type,
    string TargetUrl,
    bool IsRead,
    DateTimeOffset CreatedAt);
