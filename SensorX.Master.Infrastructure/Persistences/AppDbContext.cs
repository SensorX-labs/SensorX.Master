using System.Text.Json;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Infrastructure.Persistences;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<DomainEventOutbox> DomainEventOutboxes { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        // modelBuilder.Entity<SaleStaff>().Property<uint>("Version").IsRowVersion();
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        // Fix for PostgreSQL DateTimeOffset with offset issue
        // Npgsql 6.0+ requires DateTimeOffset to be in UTC (offset 0) when saving to 'timestamp with time zone'
        var dateTimeOffsetConverter = new ValueConverter<DateTimeOffset, DateTimeOffset>(
            v => v.ToUniversalTime(),
            v => v);

        var nullableDateTimeOffsetConverter = new ValueConverter<DateTimeOffset?, DateTimeOffset?>(
            v => v.HasValue ? v.Value.ToUniversalTime() : v,
            v => v);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTimeOffset))
                {
                    property.SetValueConverter(dateTimeOffsetConverter);
                }
                else if (property.ClrType == typeof(DateTimeOffset?))
                {
                    property.SetValueConverter(nullableDateTimeOffsetConverter);
                }
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // 1. Quét tìm các Aggregate có chứa Domain Event
        var entitiesWithEvents = ChangeTracker.Entries<IHasDomainEvents>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToList();

        // 2. Chuyển đổi Event thành Outbox Message
        foreach (var entity in entitiesWithEvents)
        {
            var domainEvents = entity.DomainEvents.ToArray();

            // Xóa event trong bộ nhớ để tránh bị lặp nếu SaveChanges chạy lại
            entity.ClearDomainEvents();

            foreach (var domainEvent in domainEvents)
            {
                var eventType = domainEvent.GetType();

                var outboxMessage = new DomainEventOutbox
                {
                    // Lấy FullName bao gồm cả namespace để Deserialize chính xác
                    EventType = eventType.AssemblyQualifiedName!,
                    Content = JsonSerializer.Serialize(domainEvent, eventType)
                };

                // Add vào Context (chưa lưu xuống DB ngay, phải chờ base.SaveChangesAsync)
                DomainEventOutboxes.Add(outboxMessage);
            }
        }

        // 3. Thực thi Transaction lưu xuống DB
        // Lúc này: Data nghiệp vụ + DomainEventOutbox + MassTransitOutbox (nếu có) sẽ được lưu CÙNG LÚC!
        return await base.SaveChangesAsync(cancellationToken);
    }
}