using System.Reflection;

namespace SensorX.Master.WebApi.Configurations
{
    public static class DI
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            // Register your services here

            // MediatR - scan từ Assembly Application
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(Assembly.Load("SensorX.Master.Application"));
            });
            return services;
        }
    }
}

