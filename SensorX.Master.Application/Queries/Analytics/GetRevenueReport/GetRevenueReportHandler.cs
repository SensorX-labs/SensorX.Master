using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

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
            DateTimeOffset startDate;
            var rangeType = request.FilterType.ToLower();

            switch (rangeType)
            {
                case "today":
                    startDate = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
                    break;
                case "week":
                    int diff = (7 + (now.DayOfWeek - DayOfWeek.Monday)) % 7;
                    var startOfWeek = now.AddDays(-1 * diff).Date;
                    startDate = new DateTimeOffset(startOfWeek, TimeSpan.Zero);
                    break;
                case "month":
                    startDate = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                    break;
                case "year":
                    startDate = new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, TimeSpan.Zero);
                    break;
                case "3_months":
                case "12_months":
                case "6_months":
                default:
                    int monthsToQueryDefault = rangeType switch
                    {
                        "3_months" => 3,
                        "12_months" => 12,
                        _ => 6
                    };
                    var startOfMonthDefault = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                    startDate = startOfMonthDefault.AddMonths(-1 * (monthsToQueryDefault - 1));
                    break;
            }

            query = query.Where(o => o.OrderDate >= startDate && o.Status != OrderStatus.Cancelled);
            var orders = await queryExecutor.ToListAsync(query, cancellationToken);

            var monthlyDataList = new List<MonthlyReportDto>();
            var chartDataList = new List<RevenueDetailDto>();

            if (rangeType == "today")
            {
                var hourBlocks = new[] { "04h", "08h", "12h", "16h", "20h", "24h" };
                string GetHourBlock(int hour) => hour switch
                {
                    < 4 => "04h",
                    < 8 => "08h",
                    < 12 => "12h",
                    < 16 => "16h",
                    < 20 => "20h",
                    _ => "24h"
                };

                foreach (var block in hourBlocks)
                {
                    var blockOrders = orders.Where(o => GetHourBlock(o.OrderDate.Hour) == block).ToList();
                    var revenue = blockOrders.Sum(o => o.GetGrandTotal().Amount);
                    var cost = revenue * 0.3m;
                    var profit = revenue - cost;

                    chartDataList.Add(new RevenueDetailDto
                    {
                        Name = block,
                        DoanhThu = revenue / 1000000m,
                        ChiPhi = cost / 1000000m,
                        LoiNhuan = profit / 1000000m
                    });
                }
            }
            else if (rangeType == "week")
            {
                var daysOfWeek = new[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
                string GetDayNameVi(DayOfWeek day) => day switch
                {
                    DayOfWeek.Monday => "T2",
                    DayOfWeek.Tuesday => "T3",
                    DayOfWeek.Wednesday => "T4",
                    DayOfWeek.Thursday => "T5",
                    DayOfWeek.Friday => "T6",
                    DayOfWeek.Saturday => "T7",
                    DayOfWeek.Sunday => "CN",
                    _ => ""
                };

                foreach (var day in daysOfWeek)
                {
                    var dayOrders = orders.Where(o => GetDayNameVi(o.OrderDate.DayOfWeek) == day).ToList();
                    var revenue = dayOrders.Sum(o => o.GetGrandTotal().Amount);
                    var cost = revenue * 0.3m;
                    var profit = revenue - cost;

                    chartDataList.Add(new RevenueDetailDto
                    {
                        Name = day,
                        DoanhThu = revenue / 1000000m,
                        ChiPhi = cost / 1000000m,
                        LoiNhuan = profit / 1000000m
                    });
                }
            }
            else if (rangeType == "month")
            {
                int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                for (int d = 1; d <= daysInMonth; d++)
                {
                    var dayOrders = orders.Where(o => o.OrderDate.Day == d).ToList();
                    var revenue = dayOrders.Sum(o => o.GetGrandTotal().Amount);
                    var cost = revenue * 0.3m;
                    var profit = revenue - cost;

                    chartDataList.Add(new RevenueDetailDto
                    {
                        Name = d.ToString(),
                        DoanhThu = revenue / 1000000m,
                        ChiPhi = cost / 1000000m,
                        LoiNhuan = profit / 1000000m
                    });
                }
            }
            else // year, all, or standard month intervals
            {
                int monthsToQuery = 6;
                if (rangeType == "year") monthsToQuery = 12;
                else if (rangeType == "3_months") monthsToQuery = 3;
                else if (rangeType == "12_months") monthsToQuery = 12;

                var startOfMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
                for (int i = monthsToQuery - 1; i >= 0; i--)
                {
                    var targetMonth = startOfMonth.AddMonths(-1 * i);
                    var monthLabel = targetMonth.ToString("MM/yyyy", CultureInfo.InvariantCulture);

                    var monthOrders = orders
                        .Where(o => o.OrderDate.Year == targetMonth.Year && o.OrderDate.Month == targetMonth.Month)
                        .ToList();

                    var revenue = monthOrders.Sum(o => o.GetGrandTotal().Amount);
                    var cost = revenue * 0.3m;
                    var profit = revenue - cost;

                    monthlyDataList.Add(new MonthlyReportDto
                    {
                        Month = $"Tháng {monthLabel}",
                        Revenue = revenue,
                        Cost = cost,
                        Profit = profit,
                        Growth = "0%"
                    });

                    chartDataList.Add(new RevenueDetailDto
                    {
                        Name = $"Th{targetMonth.Month}",
                        DoanhThu = revenue / 1000000m,
                        ChiPhi = cost / 1000000m,
                        LoiNhuan = profit / 1000000m
                    });
                }

                // Calculate Growth Rates for monthly intervals
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
            }

            // Stats for current period
            decimal totalPeriodRevenue = orders.Sum(o => o.GetGrandTotal().Amount);
            decimal totalPeriodCost = totalPeriodRevenue * 0.3m;
            decimal totalPeriodProfit = totalPeriodRevenue - totalPeriodCost;

            decimal currPeriodRevenue = rangeType switch
            {
                "today" or "week" or "month" => totalPeriodRevenue,
                _ => monthlyDataList.LastOrDefault()?.Revenue ?? 0
            };
            decimal currPeriodCost = rangeType switch
            {
                "today" or "week" or "month" => totalPeriodCost,
                _ => monthlyDataList.LastOrDefault()?.Cost ?? 0
            };
            decimal currPeriodProfit = rangeType switch
            {
                "today" or "week" or "month" => totalPeriodProfit,
                _ => monthlyDataList.LastOrDefault()?.Profit ?? 0
            };

            // Growth compared to previous period (simulated or historical fallback)
            decimal growthRate = 0;
            if (monthlyDataList.Count > 1)
            {
                var prevMonthRevenue = monthlyDataList[monthlyDataList.Count - 2].Revenue;
                if (prevMonthRevenue > 0)
                {
                    growthRate = ((currPeriodRevenue - prevMonthRevenue) / prevMonthRevenue) * 100m;
                }
            }

            var response = new GetRevenueReportResponse
            {
                MonthlyRevenue = currPeriodRevenue,
                MonthlyCost = currPeriodCost,
                MonthlyProfit = currPeriodProfit,
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
