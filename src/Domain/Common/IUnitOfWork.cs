using System.Threading;
using System.Threading.Tasks;

namespace SensorX.Master.Domain.Common;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
