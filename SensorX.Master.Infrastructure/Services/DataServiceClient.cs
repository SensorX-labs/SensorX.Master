using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.Models.DataServiceModels;

namespace SensorX.Master.Infrastructure.Services;

public class DataServiceClient : IDataServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DataServiceClient> _logger;

    public DataServiceClient(HttpClient httpClient, IConfiguration configuration, ILogger<DataServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        var baseUrl = configuration["ExternalServices:DataApi:BaseUrl"] ?? "http://localhost:5000";
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<CustomerBuyingHistoryApiResponse> GetCustomerHistoryAsync(Guid customerId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/customers/{customerId}/buying-history");
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new CustomerBuyingHistoryApiResponse { Success = false, Message = "Không tìm thấy khách hàng" };

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<CustomerBuyingHistoryApiResponse>() 
                   ?? new CustomerBuyingHistoryApiResponse { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi gọi API Customer History cho {Id}", customerId);
            return new CustomerBuyingHistoryApiResponse { Success = false, Message = ex.Message };
        }
    }

    public async Task<ProductPricingPolicyData[]> GetProductPricingAsync(Guid[] productIds)
    {
        try
        {
            if (productIds == null || productIds.Length == 0) return Array.Empty<ProductPricingPolicyData>();

            var requestBody = new { productIds = productIds };
            var response = await _httpClient.PostAsJsonAsync("/api/catalog/products/pricing-policy/batch", requestBody);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("API Product Batch trả về lỗi {Status}: {Body}", response.StatusCode, errorBody);
                return Array.Empty<ProductPricingPolicyData>();
            }

            var batchResult = await response.Content.ReadFromJsonAsync<ProductPricingBatchResponse>();
            
            if (batchResult != null && batchResult.IsSuccess)
            {
                return batchResult.Value.ToArray();
            }

            return Array.Empty<ProductPricingPolicyData>();
        }
        catch (Exception ex)
        {
             _logger.LogError(ex, "Lỗi giải mã JSON từ Product Batch API");
             return Array.Empty<ProductPricingPolicyData>();
        }
    }

    public async Task<StaffMetricsApiResponse> GetEmployeeMetricsAsync(Guid employeeId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/staff/{employeeId}/metrics");
            if (!response.IsSuccessStatusCode) 
                return new StaffMetricsApiResponse { Success = false, Message = "Lỗi gọi API Staff" };

            return await response.Content.ReadFromJsonAsync<StaffMetricsApiResponse>() 
                   ?? new StaffMetricsApiResponse { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi gọi API Staff Metrics cho {Id}", employeeId);
            return new StaffMetricsApiResponse { Success = false, Message = ex.Message };
        }
    }
}