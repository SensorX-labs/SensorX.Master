using SensorX.Master.Application.Common.Models.DataServiceModels;

namespace SensorX.Master.Application.Common.Interfaces;

public interface IDataServiceClient
{
    Task<CustomerBuyingHistoryApiResponse> GetCustomerHistoryAsync(Guid customerId);
    Task<ProductPricingPolicyData[]> GetProductPricingAsync(Guid[] productIds);
    Task<StaffMetricsApiResponse> GetEmployeeMetricsAsync(Guid employeeId);
}