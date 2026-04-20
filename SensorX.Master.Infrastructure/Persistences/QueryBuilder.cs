using Microsoft.EntityFrameworkCore;
using SensorX.Master.Application.Common.Interfaces;

namespace SensorX.Master.Infrastructure.Persistences;

public class QueryBuilder<T>(AppDbContext dbContext) : IQueryBuilder<T> where T : class
{
    private readonly AppDbContext _dbContext = dbContext;

    public IQueryable<T> Query => _dbContext.Set<T>();

    public IQueryable<T> QueryAsNoTracking => _dbContext.Set<T>().AsNoTracking();
}
