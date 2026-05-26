using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Events.IntegrationEvents.WarehouseInventory;
using SensorX.Master.Application.Services;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Infrastructure.Services.Telegram;
using Telegram.Bot;
using Microsoft.Extensions.Options;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;
using SensorX.Master.Infrastructure.Jobs;
using SensorX.Master.Infrastructure.Persistences;
using SensorX.Master.Infrastructure.Repositories;
using SensorX.Master.Infrastructure.Services;
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

            services.AddServices();
            services.AddMassTransit(configuration);
            services.AddQuartzJob(configuration);

            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IQueryBuilder<>), typeof(QueryBuilder<>));
            services.AddScoped<IQueryExecutor, QueryExecutor>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IWarehouseQueryService, WarehouseQueryService>(); // Add Query Service
            services.AddScoped<IInventoryAvailabilityService, InventoryAvailabilityService>();
            services.AddScoped<ISepayQRBuilder, SepayQRBuilder>();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<IGeolocationQueryService, GeolocationQueryService>();

            // Đăng ký HttpClient cho Data Service
            services.AddHttpClient<IDataServiceClient, DataServiceClient>();
            return services;
        }

        private static IServiceCollection AddMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("master", false));
                x.AddConsumer<SensorX.Master.Application.Events.Consumers.StaffSnapshot.StaffSnapshotConsumer>();
                x.AddConsumer<SensorX.Master.Application.Events.Consumers.ProductSnapshot.ProductSnapshotConsumer>();
                x.AddConsumer<SensorX.Master.Application.Events.Consumers.CustomerSnapshot.CustomerSnapshotConsumer>();
                x.AddConsumer<InventorySnapshotEventConsumer>();
                x.AddConsumer<WarehouseConnectedEventConsumer>();
                x.AddConsumer<SensorX.Master.Application.Events.Consumers.StockInCreatedConsumer>();
                x.AddConsumer<SensorX.Master.Application.Events.Consumers.StockOutCreatedConsumer>();

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
                        // Optimize for real-time inventory updates:
                        // - Low prefetch for immediate processing (default is 10)
                        // - Concurrent delivery for higher throughput
                        // - Immediate message acknowledgment
                        e.PrefetchCount = 1;  // Process one message at a time for ordering
                        e.ConcurrentMessageLimit = 1;  // Ensure events processed sequentially
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
            return services;
        }

        private static IServiceCollection AddQuartzJob(this IServiceCollection services, IConfiguration configuration)
        {
            // Đăng ký Quartz
            services.AddQuartz(q =>
            {
                // Tạo JobKey
                var jobKey = new JobKey(nameof(ProcessDomainEventsJob));

                // Đăng ký Job vào DI container của Quartz
                q.AddJob<ProcessDomainEventsJob>(opts => opts.WithIdentity(jobKey));

                // Lên lịch (Trigger) cho Job chạy lặp đi lặp lại
                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity($"{nameof(ProcessDomainEventsJob)}-trigger")
                    // Chạy mỗi 5 giây, mãi mãi
                    .WithSimpleSchedule(schedule => schedule
                        .WithIntervalInSeconds(5)
                        .RepeatForever())
                );
            });

            // Chạy Quartz dưới dạng Hosted Service
            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            // Đăng ký Telegram Bot + 9router Intent Router
            services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.SectionName));
            var telegramEnabled = configuration.GetValue<bool>("Telegram:Enabled");
            if (telegramEnabled)
            {
                services.AddSingleton<ITelegramBotClient>(sp =>
                {
                    var options = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
                    if (string.IsNullOrEmpty(options.Token))
                        return new TelegramBotClient("DUMMY_TOKEN");
                    return new TelegramBotClient(options.Token);
                });
                services.AddHttpClient("NineRouter");
                services.AddHostedService<TelegramBotBackgroundService>();
            }

            return services;

        }
    }
}

