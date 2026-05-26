using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SensorX.Master.Application.DTOs;
using SensorX.Master.Application.Services;
using SensorX.Master.Infrastructure.Persistences;

namespace SensorX.Master.Infrastructure.Services;

public class WarehouseQueryService : IWarehouseQueryService
{
    private readonly AppDbContext _dbContext;

    public WarehouseQueryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Warehouses
            .AsNoTracking()
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => new WarehouseDto(
                w.Id.Value,
                w.Name,
                w.Address,
                w.IsActive,
                w.Location.Latitude,
                w.Location.Longitude,
                w.CreatedAt,
                w.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Warehouses
            .AsNoTracking()
            .Where(w => w.Id.Value == id)
            .Select(w => new WarehouseDto(
                w.Id.Value,
                w.Name,
                w.Address,
                w.IsActive,
                w.Location.Latitude,
                w.Location.Longitude,
                w.CreatedAt,
                w.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<WarehouseInventoryRowDto>> GetTotalInventoryRowsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.WarehouseInventoryProjections
            .AsNoTracking()
            .OrderByDescending(p => p.LastSyncAt)
            .Select(p => new WarehouseInventoryRowDto(
                p.WarehouseId,
                p.ProductId,
                p.ProductCode,
                p.ProductName,
                p.Unit,
                p.PhysicalQuantity,
                p.AllocatedQuantity,
                p.WarehouseName,
                p.BrandZone,
                p.RackCode,
                p.LastSyncAt))
            .ToListAsync(cancellationToken);
    }


      private static readonly HttpClient _httpClient = new HttpClient();

    public async Task<WarehouseDto?> FindNearestWarehouseAsync(double lat, double lon, CancellationToken ct = default)
    {
        var warehouses = await GetAllAsync(ct);

        WarehouseDto? nearest = null;
        double minDistance = double.MaxValue;

        foreach (var w in warehouses.Where(w => w.IsActive && w.Latitude.HasValue && w.Longitude.HasValue))
        {
            var wLat = w.Latitude!.Value;
            var wLon = w.Longitude!.Value;

            // Try OSRM first; fallback to Haversine if OSRM fails or times out
            var distance = await GetOsrmDistanceAsync(lat, lon, wLat, wLon, ct);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = w;
            }
        }

        return nearest ?? warehouses.FirstOrDefault(w => w.IsActive); // Fallback to first active if no coords
    }

    private async Task<double> GetOsrmDistanceAsync(double lat1, double lon1, double lat2, double lon2, CancellationToken ct)
    {
        var lon1Str = lon1.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var lat1Str = lat1.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var lon2Str = lon2.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var lat2Str = lat2.ToString(System.Globalization.CultureInfo.InvariantCulture);

        string url = $"https://router.project-osrm.org/route/v1/driving/{lon1Str},{lat1Str};{lon2Str},{lat2Str}?overview=false";

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var response = await _httpClient.GetAsync(url, cts.Token);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cts.Token);
                using var document = System.Text.Json.JsonDocument.Parse(content);
                var root = document.RootElement;
                if (root.TryGetProperty("code", out var codeProp) && codeProp.GetString() == "Ok")
                {
                    if (root.TryGetProperty("routes", out var routesProp) && routesProp.GetArrayLength() > 0)
                    {
                        var route = routesProp[0];
                        if (route.TryGetProperty("distance", out var distanceProp))
                        {
                            return distanceProp.GetDouble(); // Distance in meters
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log exception here if logger was injected, for now just skip
            Console.WriteLine($"OSRM API Error: {ex.Message}");
        }

        // Fallback: return straight-line (Haversine) distance in meters
        return HaversineDistanceMeters(lat1, lon1, lat2, lon2);
    }

    private static double HaversineDistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371000; // Earth radius in meters
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double deg) => deg * (Math.PI / 180.0);
}