using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

namespace SensorX.Master.Application.Queries.Analytics.GetDashboardTransactionStats;

public class GetDashboardTransactionStatsHandler(
    IQueryBuilder<Order> orderQueryBuilder,
    IQueryExecutor queryExecutor
) : IRequestHandler<GetDashboardTransactionStatsQuery, Result<GetDashboardTransactionStatsResponse>>
{
    public async Task<Result<GetDashboardTransactionStatsResponse>> Handle(
        GetDashboardTransactionStatsQuery request,

        CancellationToken cancellationToken)
    {
        try
        {
            var query = orderQueryBuilder.QueryAsNoTracking;
            var vnOffset = TimeSpan.FromHours(7);
            var vnNow = DateTimeOffset.UtcNow.ToOffset(vnOffset);
            var now = vnNow;
            DateTimeOffset? startDate = null;
            DateTimeOffset? prevStartDate = null;
            DateTimeOffset? prevEndDate = null;

            switch (request.TimeRange.ToLower())
            {
                case "today":
                    startDate = new DateTimeOffset(vnNow.Year, vnNow.Month, vnNow.Day, 0, 0, 0, vnOffset);
                    prevStartDate = startDate.Value.AddDays(-1);
                    prevEndDate = prevStartDate.Value.Add(vnNow - startDate.Value);
                    break;
                case "week":
                    // Monday of current week
                    int diff = (7 + (vnNow.DayOfWeek - DayOfWeek.Monday)) % 7;
                    var startOfWeek = vnNow.AddDays(-diff).Date;
                    startDate = new DateTimeOffset(startOfWeek.Year, startOfWeek.Month, startOfWeek.Day, 0, 0, 0, vnOffset);
                    prevStartDate = startDate.Value.AddDays(-7);
                    prevEndDate = prevStartDate.Value.Add(vnNow - startDate.Value);
                    break;
                case "month":
                    startDate = new DateTimeOffset(vnNow.Year, vnNow.Month, 1, 0, 0, 0, vnOffset);
                    prevStartDate = startDate.Value.AddMonths(-1);
                    prevEndDate = prevStartDate.Value.Add(vnNow - startDate.Value);
                    break;
                case "year":
                    startDate = new DateTimeOffset(vnNow.Year, 1, 1, 0, 0, 0, vnOffset);
                    prevStartDate = startDate.Value.AddYears(-1);
                    prevEndDate = prevStartDate.Value.Add(vnNow - startDate.Value);
                    break;
                case "all":
                default:
                    break;
            }

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value && o.OrderDate <= vnNow);
            }

            var orders = await queryExecutor.ToListAsync(query, cancellationToken);

            var prevOrders = new List<Order>();
            if (prevStartDate.HasValue && prevEndDate.HasValue)
            {
                var prevQuery = orderQueryBuilder.QueryAsNoTracking
                    .Where(o => o.OrderDate >= prevStartDate.Value && o.OrderDate < prevEndDate.Value);
                prevOrders = await queryExecutor.ToListAsync(prevQuery, cancellationToken);
            }

            var totalOrders = orders.Count;
            var totalRevenue = orders.Sum(o => o.GetGrandTotal().Amount);
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var prevTotalOrders = prevOrders.Count;
            var prevTotalRevenue = prevOrders.Sum(o => o.GetGrandTotal().Amount);
            var prevAverageOrderValue = prevTotalOrders > 0 ? prevTotalRevenue / prevTotalOrders : 0;

            // Group by Product
            var topProducts = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => new { i.ProductId, i.ProductCode, i.ProductName })
                .Select(g => new TopProductDto
                {
                    ProductId = g.Key.ProductId.Value,
                    ProductCode = g.Key.ProductCode.Value,
                    ProductName = g.Key.ProductName,
                    QuantitySold = g.Sum(i => i.Quantity.Value),
                    TotalAmount = g.Sum(i => i.GetTotalLineAmount().Amount)
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(5)
                .ToList();

            // Dynamic Order Frequency Sales aggregation based on selected TimeRange
            var weeklySales = new List<WeeklySalesDto>();
            var rangeType = request.TimeRange.ToLower();

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

                var ordersByHour = orders
                    .GroupBy(o => GetHourBlock(o.OrderDate.Hour))
                    .ToDictionary(g => g.Key, g => (decimal)g.Count());

                foreach (var block in hourBlocks)
                {
                    weeklySales.Add(new WeeklySalesDto
                    {
                        Day = block,
                        Value = ordersByHour.TryGetValue(block, out var val) ? val : 0
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

                var ordersByDay = orders
                    .GroupBy(o => o.OrderDate.DayOfWeek)
                    .ToDictionary(g => GetDayNameVi(g.Key), g => (decimal)g.Count());

                foreach (var day in daysOfWeek)
                {
                    weeklySales.Add(new WeeklySalesDto
                    {
                        Day = day,
                        Value = ordersByDay.TryGetValue(day, out var val) ? val : 0
                    });
                }
            }
            else if (rangeType == "month")
            {
                int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                var ordersByDay = orders
                    .GroupBy(o => o.OrderDate.Day)
                    .ToDictionary(g => g.Key, g => (decimal)g.Count());

                for (int d = 1; d <= daysInMonth; d++)
                {
                    weeklySales.Add(new WeeklySalesDto
                    {
                        Day = d.ToString(),
                        Value = ordersByDay.TryGetValue(d, out var val) ? val : 0
                    });
                }
            }
            else // year or all
            {
                var months = new[] { "Th1", "Th2", "Th3", "Th4", "Th5", "Th6", "Th7", "Th8", "Th9", "Th10", "Th11", "Th12" };
                var ordersByMonth = orders
                    .GroupBy(o => o.OrderDate.Month)
                    .ToDictionary(g => $"Th{g.Key}", g => (decimal)g.Count());

                foreach (var m in months)
                {
                    weeklySales.Add(new WeeklySalesDto
                    {
                        Day = m,
                        Value = ordersByMonth.TryGetValue(m, out var val) ? val : 0
                    });
                }
            }

            var response = new GetDashboardTransactionStatsResponse
            {
                TotalRevenue = totalRevenue,
                AverageOrderValue = averageOrderValue,
                TotalOrders = totalOrders,
                PreviousTotalOrders = prevTotalOrders,
                PreviousAverageOrderValue = prevAverageOrderValue,
                TopSellingProducts = topProducts,
                WeeklySales = weeklySales
            };

            return Result<GetDashboardTransactionStatsResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<GetDashboardTransactionStatsResponse>.Failure($"Loi khi thong ke giao dich: {ex.Message}");
        }
    }
}
