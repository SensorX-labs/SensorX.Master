using System.ComponentModel.DataAnnotations;

namespace SensorX.Master.Domain.SeedWork; // Hoặc namespace phù hợp

public class DomainEventOutbox
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    // Lưu AssemblyQualifiedName để lúc Deserialize biết nó là class nào
    public required string EventType { get; set; }

    // Lưu toàn bộ data của event dưới dạng JSON
    public required string Content { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Đánh dấu thời điểm xử lý xong. Nếu Null nghĩa là chưa xử lý.
    public DateTimeOffset? ProcessedAt { get; set; }

    // Lưu lại lỗi nếu lúc bắn qua MediatR bị crash
    public string? Error { get; set; }
}