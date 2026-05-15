using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Events.IntegrationEvents.QuoteAnalysis;
using SensorX.Master.Application.Services;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Infrastructure.Jobs;
using SensorX.Master.Infrastructure.Persistences;
using SensorX.Master.Infrastructure.Repositories;
using SensorX.Master.Infrastructure.Services;

namespace SensorX.Master.Infrastructure.DI
{
    public static class DI
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddServices();
            services.AddMassTransit(configuration);
            services.AddQuartzJob();

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
            services.AddScoped<ICurrentUser, CurrentUser>();

            // Đăng ký HttpClient cho Data Service
            services.AddHttpClient<IDataServiceClient, DataServiceClient>();
            return services;
        }

        private static IServiceCollection AddMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMassTransit(x =>
            {
                x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("master", false));
                // Đăng ký Consumer chạy ngầm
                x.AddConsumer<SensorX.Master.Application.Events.IntegrationEvents.QuoteAnalysis.QuoteAnalysisIntegrationEvent>();
                x.AddConsumer<SensorX.Master.Application.Events.Consumers.StaffSnapshot.StaffSnapshotConsumer>();
                x.AddConsumer<SensorX.Master.Application.Events.Consumers.ProductSnapshot.ProductSnapshotConsumer>();
                x.AddConsumer<SensorX.Master.Application.Events.Consumers.CustomerSnapshot.CustomerSnapshotConsumer>();

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

                    cfg.ConfigureEndpoints(context);
                });


            });
            return services;
        }

        private static IServiceCollection AddQuartzJob(this IServiceCollection services)
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

            return services;
        }
    }
}

