using System;

namespace SensorX.Master.Domain.Common;

public class NotificationEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid? UserId { get; private set; }       // AccountId of the recipient (optional if role is set)
    public string? Role { get; private set; }       // Target role (optional if userId is set)
    public string Title { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public string Type { get; private set; } = default!;       // "RFQ"|"Quote"|"Order"|"Warehouse"
    public string TargetUrl { get; private set; } = default!;  // Redirect URL on click
    public bool IsRead { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private NotificationEntity() { }

    public void MarkAsRead() => IsRead = true;

    public static NotificationEntity CreateForUser(Guid userId, string title, string content, string type, string targetUrl)
        => new()
        {
            UserId = userId,
            Title = title,
            Content = content,
            Type = type,
            TargetUrl = targetUrl,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        };

    public static NotificationEntity CreateForRole(string role, string title, string content, string type, string targetUrl)
        => new()
        {
            Role = role,
            Title = title,
            Content = content,
            Type = type,
            TargetUrl = targetUrl,
            IsRead = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
}
