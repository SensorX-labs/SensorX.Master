using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Events.IntegrationEvents.QuoteAnalysis;
using SensorX.Master.Application.Services;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Infrastructure.Persistences;
using SensorX.Master.Infrastructure.Services;
using SensorX.Master.Infrastructure.Services.Telegram;
using Telegram.Bot;
using Microsoft.Extensions.Options;


using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;
using SensorX.Master.Infrastructure.Repositories;

namespace SensorX.Master.Infrastructure.DI
{
    public static class DI
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            services.AddMassTransit(x =>
            {
                // Đăng ký Consumer chạy ngầm
                x.AddConsumer<QuoteAnalysisIntegrationEvent>();

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

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IQueryBuilder<>), typeof(QueryBuilder<>));
            services.AddScoped<IQueryExecutor, QueryExecutor>();
            services.AddScoped<IWarehouseRepository, WarehouseRepository>();
            services.AddScoped<IWarehouseQueryService, WarehouseQueryService>(); // Add Query Service
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICurrentUser, CurrentUser>();

            // Đăng ký HttpClient cho Data Service
            services.AddHttpClient<IDataServiceClient, DataServiceClient>();

            // Đăng ký Telegram Bot
            services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.SectionName));
            services.AddSingleton<ITelegramBotClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
                if (string.IsNullOrEmpty(options.Token))
                {
                    // Tránh crash nếu chưa cấu hình token, nhưng log cảnh báo
                    return new TelegramBotClient("DUMMY_TOKEN"); 
                }
                return new TelegramBotClient(options.Token);
            });
            services.AddHostedService<TelegramBotBackgroundService>();

            return services;

        }
    }
}

