using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Services
{
    public interface IGeolocationQueryService
    {
        Task<List<Geolocation?>?> GetGeolocationByAddress(string address, CancellationToken cancellationToken);
    }
}