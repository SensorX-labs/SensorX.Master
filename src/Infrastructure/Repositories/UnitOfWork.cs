using System.Threading;
using System.Threading.Tasks;
using SensorX.Master.Domain.Common;
using SensorX.Master.Infrastructure.Persistence;

namespace SensorX.Master.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
