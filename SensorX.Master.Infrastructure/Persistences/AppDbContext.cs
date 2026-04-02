using MediatR;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Infrastructure.Persistences;

public class AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator) : DbContext(options)
{
    private readonly IMediator _mediator = mediator;
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            var entitiesWithEvents = ChangeTracker.Entries<IHasDomainEvents>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Count > 0)
                .ToList();

            if (entitiesWithEvents.Count == 0) break;

            foreach (var entity in entitiesWithEvents)
            {
                var domainEvents = entity.DomainEvents.ToArray();
                entity.ClearDomainEvents();

                foreach (var domainEvent in domainEvents)
                {
                    var notification = (INotification)Activator.CreateInstance(
                        typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType()),
                        domainEvent
                    )!;

                    await _mediator.Publish(notification, cancellationToken);
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}