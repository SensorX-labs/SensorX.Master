namespace SensorX.Master.Application.Common.Models.DataServiceModels;

public class CustomerBuyingHistoryApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    
    public CustomerBuyingHistoryData? Data { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class CustomerBuyingHistoryData
{
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
