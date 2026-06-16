using System.Reflection;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.Services;
using SensorX.Master.WebApi.Services;

namespace SensorX.Master.WebApi.Configurations
{
    public static class DI
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register your services here

            // MediatR - scan từ Assembly Application
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.Load("SensorX.Master.Application"));
                cfg.LicenseKey = configuration["MediatR:LicenseKey"];
            });

            services.AddScoped<OrderService>();
            services.AddScoped<IPaymentNotificationService, PaymentNotificationService>();
            services.AddScoped<IRealtimeNotificationService, RealtimeNotificationService>();
            return services;
        }
    }
}

