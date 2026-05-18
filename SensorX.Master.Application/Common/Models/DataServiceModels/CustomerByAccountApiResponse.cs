namespace SensorX.Master.Application.Common.Models.DataServiceModels;

public class CustomerByAccountApiResponse
{
    public bool IsSuccess { get; set; }
    public CustomerByAccountData? Value { get; set; }
    public string? Message { get; set; }
}

public class CustomerByAccountData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
