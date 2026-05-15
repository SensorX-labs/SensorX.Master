using Dapper;
using Microsoft.Extensions.Configuration;
using SensorX.Master.Application.DTOs;
using SensorX.Master.Application.Services;
using Npgsql;

namespace SensorX.Master.Infrastructure.Services;

public class WarehouseQueryService : IWarehouseQueryService
{
    private readonly string _connectionString;

    public WarehouseQueryService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<List<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            SELECT 
                w.""Id"" AS Id,
                w.""Name"" AS Name,
                w.""Address"" AS Address,
                w.""ApiEndpointUrl"" AS ApiEndpointUrl,
                w.""IsActive"" AS IsActive,
                w.""CreatedAt"" AS CreatedAt,
                w.""UpdatedAt"" AS UpdatedAt
            FROM ""Warehouses"" w
            ORDER BY w.""CreatedAt"" DESC";

        var warehouses = await connection.QueryAsync<WarehouseDto>(sql);
        return warehouses.ToList();
    }

    public async Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var sql = @"
            SELECT 
                w.""Id"" AS Id,
                w.""Name"" AS Name,
                w.""Address"" AS Address,
                w.""ApiEndpointUrl"" AS ApiEndpointUrl,
                w.""IsActive"" AS IsActive,
                w.""CreatedAt"" AS CreatedAt,
                w.""UpdatedAt"" AS UpdatedAt
            FROM ""Warehouses"" w
            WHERE w.""Id"" = @Id";

        return await connection.QueryFirstOrDefaultAsync<WarehouseDto>(sql, new { Id = id });
    }
}