namespace SensorX.Master.Application.Common.Models.DataServiceModels;

public class StaffMetricsApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    
    public StaffMetricsData? Data { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class StaffMetricsData
{
    public Guid StaffId { get; set; }
    public string StaffCode { get; set; } = string.Empty;
    public string StaffName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
