using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Events.IntegrationEvents.QuoteAnalysis;
using SensorX.Master.Application.Events.IntegrationEvents.WarehouseInventory;
using SensorX.Master.Application.Services;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Infrastructure.Persistences;
using SensorX.Master.Infrastructure.Services;

using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;
using SensorX.Master.Infrastructure.Repositories;
using SensorX.Warehouse.Application.Events;

namespace SensorX.Master.Infrastructure.DI
{
    public static class DI
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
                       .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

            services.AddMassTransit(x =>
            {
                // Đăng ký Consumer chạy ngầm
                x.AddConsumer<QuoteAnalysisIntegrationEvent>();
                x.AddConsumer<InventorySnapshotEventConsumer>();
                x.AddConsumer<WarehouseConnectedEventConsumer>();

                // Đăng ký Entity Framework Outbox
                x.AddEntityFrameworkOutbox<AppDbContext>(o =>
                {
                    o.UsePostgres();
                    o.UseBusOutbox();
                });

                x.UsingRabbitMq((context, cfg) =>
                {
                    var rabbitMqSettings = configuration.GetSection("RabbitMq");
                    var host = rabbitMqSettings["Host"] ?? "localhost";
                    var port = ushort.Parse(rabbitMqSettings["Port"] ?? "5672");
                    var virtualHost = rabbitMqSettings["VirtualHost"] ?? "/";

                    cfg.Host(host, port, virtualHost, h =>
                    {
                        h.Username(rabbitMqSettings["Username"] ?? "guest");
                        h.Password(rabbitMqSettings["Password"] ?? "guest");
                    });

                    cfg.ReceiveEndpoint("master-inventory-snapshot-consumer", e =>
                    {
                        e.ConfigureConsumer<InventorySnapshotEventConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("master-warehouse-connected-consumer", e =>
                    {
                        e.ConfigureConsumer<WarehouseConnectedEventConsumer>(context);
                    });

                    cfg.ConfigureEndpoints(context);

                    cfg.Message<InventorySnapshotEvent>(e => e.SetEntityName("Inventory-Snapshot-Event"));
                    cfg.Message<WarehouseConnectedEvent>(e => e.SetEntityName("Warehouse-Connected-Event"));
                });


            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IQueryBuilder<>), typeof(QueryBuilder<>));
            services.AddScoped<IQueryExecutor, QueryExecutor>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IWarehouseQueryService, WarehouseQueryService>(); // Add Query Service
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<IGeolocationQueryService, GeolocationQueryService>();

            // Đăng ký HttpClient cho Data Service
            services.AddHttpClient<IDataServiceClient, DataServiceClient>();

            return services;
        }
    }
}

