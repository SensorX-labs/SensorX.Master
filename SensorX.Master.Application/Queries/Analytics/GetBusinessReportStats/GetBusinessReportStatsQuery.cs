using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using System;
using System.Collections.Generic;

namespace SensorX.Master.Application.Queries.Analytics.GetBusinessReportStats;

public record GetBusinessReportStatsQuery(
    string TimeRange = "month" // today, week, month, year, all
) : IRequest<Result<BusinessReportStatsResponse>>;

public class BusinessReportStatsResponse 
{
    // 1. Core
    public decimal TotalRevenue { get; set; }
    public decimal GrossProfit { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    
    // 2. Conversion
    public int TotalQuotes { get; set; }
    public int ConvertedQuotes { get; set; }
    public double ConversionRate { get; set; }

    // 3. Customers
    public int NewCustomers { get; set; }
    public int ReturningCustomers { get; set; }
    
    // 4. Lịch sử / Trend
    public List<CustomerRevenueDto> TopCustomers { get; set; } = new();
    public List<ProductSalesDto> TopProducts { get; set; } = new();
    public List<RevenueTrendDto> RevenueTrendChart { get; set; } = new(); 
}

public class CustomerRevenueDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public decimal Revenue { get; set; }
}

public class ProductSalesDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class RevenueTrendDto
{
    public string Period { get; set; } = null!;
    public decimal Revenue { get; set; }
    public decimal Profit { get; set; }
}
