using SensorX.Master.Infrastructure.Persistence;
using SensorX.Master.Infrastructure.Repositories;

namespace SensorX.Master.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
