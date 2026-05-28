using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SensorX.Master.Application.Queries.Analytics.GetRevenueReport;

public class GetRevenueReportHandler(
    IQueryBuilder<Order> orderQueryBuilder,
    IQueryExecutor queryExecutor
) : IRequestHandler<GetRevenueReportQuery, Result<GetRevenueReportResponse>>
{
    public async Task<Result<GetRevenueReportResponse>> Handle(
        GetRevenueReportQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var query = orderQueryBuilder.QueryAsNoTracking;
            var now = DateTimeOffset.UtcNow;
            
            int monthsToQuery = request.FilterType.ToLower() switch
            {
                "3_months" => 3,
                "12_months" => 12,
                "6_months" or _ => 6
            };

            // Start of the period (e.g. 6 months ago)
            var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var startDate = startOfMonth.AddMonths(-1 * (monthsToQuery - 1));

            query = query.Where(o => o.OrderDate >= startDate && o.Status != OrderStatus.Cancelled);

            var orders = await queryExecutor.ToListAsync(query, cancellationToken);

            var monthlyDataList = new List<MonthlyReportDto>();
            var chartDataList = new List<RevenueDetailDto>();

            // Generate list of months in scope
            for (int i = monthsToQuery - 1; i >= 0; i--)
            {
                var targetMonth = startOfMonth.AddMonths(-1 * i);
                var monthLabel = targetMonth.ToString("MM/yyyy", CultureInfo.InvariantCulture);

                var monthOrders = orders
                    .Where(o => o.OrderDate.Year == targetMonth.Year && o.OrderDate.Month == targetMonth.Month)
                    .ToList();

                var revenue = monthOrders.Sum(o => o.GetGrandTotal().Amount);
                var cost = revenue * 0.3m; // Estimate COGS as 30% of revenue
                var profit = revenue - cost;

                monthlyDataList.Add(new MonthlyReportDto
                {
                    Month = $"Tháng {monthLabel}",
                    Revenue = revenue,
                    Cost = cost,
                    Profit = profit,
                    Growth = "0%" // Calculated later
                });

                chartDataList.Add(new RevenueDetailDto
                {
                    Name = $"Th{targetMonth.Month}",
                    DoanhThu = revenue / 1000000m, // In Millions for cleaner chart display
                    ChiPhi = cost / 1000000m,
                    LoiNhuan = profit / 1000000m
                });
            }

            // Calculate Growth Rates
            for (int i = 0; i < monthlyDataList.Count; i++)
            {
                if (i == 0)
                {
                    monthlyDataList[i].Growth = "+0.0%";
                    continue;
                }

                var prevRevenue = monthlyDataList[i - 1].Revenue;
                var currRevenue = monthlyDataList[i].Revenue;

                if (prevRevenue == 0)
                {
                    monthlyDataList[i].Growth = currRevenue > 0 ? "+100.0%" : "+0.0%";
                }
                else
                {
                    var growthPercent = ((currRevenue - prevRevenue) / prevRevenue) * 100m;
                    string sign = growthPercent >= 0 ? "+" : "";
                    monthlyDataList[i].Growth = $"{sign}{growthPercent:F1}%";
                }
            }

            // Stats for current month
            var currentMonthData = monthlyDataList.LastOrDefault();
            decimal currMonthRevenue = currentMonthData?.Revenue ?? 0;
            decimal currMonthCost = currentMonthData?.Cost ?? 0;
            decimal currMonthProfit = currentMonthData?.Profit ?? 0;

            // Growth compared to previous month
            decimal growthRate = 0;
            if (monthlyDataList.Count > 1)
            {
                var prevMonthRevenue = monthlyDataList[monthlyDataList.Count - 2].Revenue;
                if (prevMonthRevenue > 0)
                {
                    growthRate = ((currMonthRevenue - prevMonthRevenue) / prevMonthRevenue) * 100m;
                }
            }

            var response = new GetRevenueReportResponse
            {
                MonthlyRevenue = currMonthRevenue,
                MonthlyCost = currMonthCost,
                MonthlyProfit = currMonthProfit,
                GrowthRate = growthRate,
                ChartData = chartDataList,
                TableData = monthlyDataList
            };

            return Result<GetRevenueReportResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<GetRevenueReportResponse>.Failure($"Loi khi lam thong ke doanh thu: {ex.Message}");
        }
    }
}
