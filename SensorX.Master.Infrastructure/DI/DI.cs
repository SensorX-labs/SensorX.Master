using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Infrastructure.Persistences;
using SensorX.Master.Infrastructure.Services;

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
                // Đăng ký Entity Framework Outbox
                x.AddEntityFrameworkOutbox<AppDbContext>(o =>
                {
                    // Sử dụng Postgres
                    o.UsePostgres();

                    // Quan trọng: Báo cho MassTransit biết hãy đóng vai trò là Outbox
                    o.UseBusOutbox();
                });

                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });

                // Cấu hình RabbitMQ (hoặc broker khác)
                // x.UsingRabbitMq((context, cfg) =>
                // {
                //     cfg.Host("localhost", "/", h =>
                //     {
                //         h.Username("guest");
                //         h.Password("guest");
                //     });

                //     cfg.ConfigureEndpoints(context);
                // });
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IQueryBuilder<>), typeof(QueryBuilder<>));
            services.AddScoped<IQueryExecutor, QueryExecutor>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICurrentUser, CurrentUser>();

            return services;
        }
    }
}

