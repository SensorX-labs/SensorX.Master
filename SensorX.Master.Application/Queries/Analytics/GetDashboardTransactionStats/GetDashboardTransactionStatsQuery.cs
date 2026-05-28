using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using System;
using System.Collections.Generic;

namespace SensorX.Master.Application.Queries.Analytics.GetDashboardTransactionStats;

public record GetDashboardTransactionStatsQuery(
    string TimeRange = "month" // today, week, month, year, all
) : IRequest<Result<GetDashboardTransactionStatsResponse>>;

public class GetDashboardTransactionStatsResponse
{
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TotalOrders { get; set; }
    public List<TopProductDto> TopSellingProducts { get; set; } = new();
    public List<WeeklySalesDto> WeeklySales { get; set; } = new();
}

public class TopProductDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public decimal QuantitySold { get; set; }
    public decimal TotalAmount { get; set; }
}

public class WeeklySalesDto
{
    public string Day { get; set; } = null!;
    public decimal Value { get; set; }
}
