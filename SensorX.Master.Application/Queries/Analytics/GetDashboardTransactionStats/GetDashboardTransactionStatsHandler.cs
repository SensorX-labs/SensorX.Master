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
            var now = DateTimeOffset.UtcNow;
            DateTimeOffset? startDate = null;

            switch (request.TimeRange.ToLower())
            {
                case "today":
                    startDate = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);
                    break;
                case "week":
                    // Monday of current week
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
                case "all":
                default:
                    break;
            }

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }

            var orders = await queryExecutor.ToListAsync(query, cancellationToken);

            if (!orders.Any())
            {
                return Result<GetDashboardTransactionStatsResponse>.Success(new GetDashboardTransactionStatsResponse());
            }

            var totalOrders = orders.Count;
            var totalRevenue = orders.Sum(o => o.GetGrandTotal().Amount);
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

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

            // Weekly Sales (aggregate by day of week for the last 7 days)
            var weeklySales = new List<WeeklySalesDto>();
            var daysOfWeek = new[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };

            // Map DayOfWeek enum to Vietnamese short days

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
                .Where(o => o.OrderDate >= now.AddDays(-7))
                .GroupBy(o => o.OrderDate.DayOfWeek)
                .ToDictionary(g => GetDayNameVi(g.Key), g => g.Sum(o => o.GetGrandTotal().Amount));

            foreach (var day in daysOfWeek)
            {
                weeklySales.Add(new WeeklySalesDto
                {
                    Day = day,
                    Value = ordersByDay.TryGetValue(day, out var val) ? val : 0
                });
            }

            var response = new GetDashboardTransactionStatsResponse
            {
                TotalRevenue = totalRevenue,
                AverageOrderValue = averageOrderValue,
                TotalOrders = totalOrders,
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
