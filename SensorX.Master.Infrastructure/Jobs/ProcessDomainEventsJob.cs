using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;
using SensorX.Master.Application.Common.DomainEvent; // Nơi chứa DomainEventNotification của bạn
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Infrastructure.Persistences;

namespace SensorX.Master.Infrastructure.Jobs;

// Attribute CỰC KỲ QUAN TRỌNG: Ngăn Quartz chạy 2 Job cùng lúc đè lên nhau
[DisallowConcurrentExecution]
public class ProcessDomainEventsJob(AppDbContext dbContext, IMediator mediator, ILogger<ProcessDomainEventsJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // 1. Lấy ra tối đa 50 event CHƯA XỬ LÝ, ưu tiên cái cũ nhất trước
        var messages = await dbContext.DomainEventOutboxes
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(50)
            .ToListAsync(context.CancellationToken);

        if (messages.Count == 0) return;

        foreach (var message in messages)
        {
            try
            {
                // 2. Phục hồi object từ JSON
                var eventType = Type.GetType(message.EventType);
                if (eventType == null)
                {
                    message.Error = $"Không tìm thấy Type: {message.EventType}";
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Content, eventType);
                if (domainEvent == null)
                {
                    message.Error = "Deserialize thất bại, dữ liệu null.";
                    continue;
                }

                // 3. Bọc lại vào DomainEventNotification (giống hệt code cũ của bạn)
                var notification = (INotification)Activator.CreateInstance(
                    typeof(DomainEventNotification<>).MakeGenericType(eventType),
                    domainEvent
                )!;

                // 4. Publish cho MediatR xử lý
                await mediator.Publish(notification, context.CancellationToken);

                // 5. Đánh dấu thành công
                message.ProcessedAt = DateTime.Now;
                message.Error = null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi xử lý DomainEventOutbox Id: {Id}", message.Id);
                message.Error = ex.Message; // Ghi nhận lỗi để fix sau
            }
        }

        // 6. Lưu trạng thái (ProcessedAt hoặc Error) cập nhật lại vào DB
        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}