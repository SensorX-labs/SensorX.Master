namespace SensorX.Master.Application.Common.Models.DataServiceModels;

public class ProductPricingPolicyApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    
    public ProductPricingPolicyData? Data { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ProductPricingPolicyData
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Manufacture { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int ProductStatus { get; set; }
    
    public decimal FloorPrice { get; set; }
    public decimal SuggestedPrice { get; set; }
    public List<PriceTierData> PriceTiers { get; set; } = [];
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PriceTierData
{
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class ProductPricingBatchResponse
{
    public bool IsSuccess { get; set; }
    public List<ProductPricingPolicyData> Value { get; set; } = [];
}
