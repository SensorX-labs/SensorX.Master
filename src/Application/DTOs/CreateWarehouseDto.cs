namespace SensorX.Master.Application.DTOs;

public record CreateWarehouseDto(
    string Name,
    string? Address,
    string ApiEndpointUrl
);
