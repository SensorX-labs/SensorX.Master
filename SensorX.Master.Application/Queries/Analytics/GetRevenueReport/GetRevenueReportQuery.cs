using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using System.Collections.Generic;

namespace SensorX.Master.Application.Queries.Analytics.GetRevenueReport;

public record GetRevenueReportQuery(
    string FilterType = "6_months" // 3_months, 6_months, 12_months
) : IRequest<Result<GetRevenueReportResponse>>;

public class GetRevenueReportResponse
{
    public decimal MonthlyRevenue { get; set; }
    public decimal MonthlyCost { get; set; }
    public decimal MonthlyProfit { get; set; }
    public decimal GrowthRate { get; set; }
    public List<RevenueDetailDto> ChartData { get; set; } = new();
    public List<MonthlyReportDto> TableData { get; set; } = new();
}

public class RevenueDetailDto
{
    public string Name { get; set; } = null!;
    public decimal DoanhThu { get; set; }
    public decimal ChiPhi { get; set; }
    public decimal LoiNhuan { get; set; }
}

public class MonthlyReportDto
{
    public string Month { get; set; } = null!;
    public decimal Revenue { get; set; }
    public decimal Cost { get; set; }
    public decimal Profit { get; set; }
    public string Growth { get; set; } = null!;
}
