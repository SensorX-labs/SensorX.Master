using System.Reflection;
using SensorX.Master.Domain.Services;

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
            return services;
        }
    }
}

