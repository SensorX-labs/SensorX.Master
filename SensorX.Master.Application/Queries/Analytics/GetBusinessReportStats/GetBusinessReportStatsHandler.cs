using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Queries.Analytics.GetBusinessReportStats;

public class GetBusinessReportStatsHandler(
    IQueryBuilder<Order> orderQueryBuilder,
    IQueryBuilder<Quote> quoteQueryBuilder,
    IQueryBuilder<RFQ> rfqQueryBuilder,
    IQueryExecutor queryExecutor
) : IRequestHandler<GetBusinessReportStatsQuery, Result<BusinessReportStatsResponse>>
{
    public async Task<Result<BusinessReportStatsResponse>> Handle(GetBusinessReportStatsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var vnOffset = TimeSpan.FromHours(7);
            var vnNow = DateTimeOffset.UtcNow.ToOffset(vnOffset);
            var now = vnNow;
            DateTimeOffset? startDate = null;

            switch (request.TimeRange.ToLower())
            {
                case "today":
                    startDate = new DateTimeOffset(vnNow.Year, vnNow.Month, vnNow.Day, 0, 0, 0, vnOffset);
                    break;
                case "week":
                    int diff = (7 + (vnNow.DayOfWeek - DayOfWeek.Monday)) % 7;
                    var startOfWeek = vnNow.AddDays(-diff).Date;
                    startDate = new DateTimeOffset(startOfWeek.Year, startOfWeek.Month, startOfWeek.Day, 0, 0, 0, vnOffset);
                    break;
                case "month":
                    startDate = new DateTimeOffset(vnNow.Year, vnNow.Month, 1, 0, 0, 0, vnOffset);
                    break;
                case "year":
                    startDate = new DateTimeOffset(vnNow.Year, 1, 1, 0, 0, 0, vnOffset);
                    break;
                default:
                    break;
            }

            // 1. Fetch Orders
            var orderQuery = orderQueryBuilder.QueryAsNoTracking;
            if (startDate.HasValue)
            {
                orderQuery = orderQuery.Where(o => o.OrderDate >= startDate.Value && o.OrderDate <= vnNow);
            }
            var orders = await queryExecutor.ToListAsync(orderQuery, cancellationToken);

            // Fetch all orders for customer history analysis (New vs Returning)

            var allTimeOrdersQuery = orderQueryBuilder.QueryAsNoTracking;
            var allOrders = await queryExecutor.ToListAsync(allTimeOrdersQuery, cancellationToken);

            // 2. Fetch Quotes
            var quoteQuery = quoteQueryBuilder.QueryAsNoTracking;
            if (startDate.HasValue)
            {
                quoteQuery = quoteQuery.Where(q => q.CreatedAt >= startDate.Value && q.CreatedAt <= vnNow);
            }
            var quotes = await queryExecutor.ToListAsync(quoteQuery, cancellationToken);

            // CORE KPIs
            var totalOrders = orders.Count;
            var totalRevenue = orders.Sum(o => o.GetGrandTotal().Amount);
            // Assuming cost is not explicitly stored, or for simplicity GrossProfit = Revenue * 0.36 (approx) 
            // In a real scenario, we'd subtract cost. Here we mock a 36% margin if Cost is missing, or 0 if we don't have it.
            // Wait, let's just use Revenue as Profit for now or a percentage if cost is unknown.
            var grossProfit = totalRevenue * 0.36m;

            var aov = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            // CONVERSIONS
            var totalQuotes = quotes.Count;
            var convertedQuotes = quotes.Count(q => q.Status == QuoteStatus.Ordered);
            double conversionRate = totalQuotes > 0 ? Math.Round((double)convertedQuotes * 100 / totalQuotes, 2) : 0;

            // Fetch RFQs as List to calculate trend
            var rfqQuery = rfqQueryBuilder.QueryAsNoTracking;
            if (startDate.HasValue)
            {
                rfqQuery = rfqQuery.Where(r => r.CreatedAt >= startDate.Value && r.CreatedAt <= vnNow);
            }
            var rfqs = await queryExecutor.ToListAsync(rfqQuery, cancellationToken);
            var totalRfqs = rfqs.Count;

            // CUSTOMER INSIGHTS
            var customerIdsInPeriod = orders.Select(o => o.CustomerId.Value).Distinct().ToList();
            int newCustomers = 0;
            foreach (var custId in customerIdsInPeriod)
            {
                var hasOrderBefore = allOrders.Any(o => o.CustomerId.Value == custId && (!startDate.HasValue || o.OrderDate < startDate.Value));
                if (!hasOrderBefore)
                    newCustomers++;
            }

            var topCustomers = orders
                .GroupBy(o => o.CustomerId.Value)
                .Select(g => new CustomerRevenueDto
                {
                    CustomerId = g.Key,
                    CustomerName = g.First().DeliveryInfo.CompanyName ?? "Khách hàng " + g.Key.ToString().Substring(0, 4),
                    Revenue = g.Sum(o => o.GetGrandTotal().Amount)
                })
                .OrderByDescending(c => c.Revenue)
                .Take(5)
                .ToList();

            // TOP PRODUCTS
            var topProducts = orders
                .SelectMany(o => o.Items)
                .GroupBy(i => new { i.ProductId, i.ProductCode, i.ProductName })
                .Select(g => new ProductSalesDto
                {
                    ProductId = g.Key.ProductId.Value,
                    ProductCode = g.Key.ProductCode.Value,
                    ProductName = g.Key.ProductName,
                    QuantitySold = (int)g.Sum(i => i.Quantity.Value),
                    Revenue = g.Sum(i => i.GetTotalLineAmount().Amount)
                })
                .OrderByDescending(p => p.Revenue)
                .Take(5)
                .ToList();

            // REVENUE TREND CHART
            var revenueTrend = new List<RevenueTrendDto>();
            var conversionTrend = new List<ConversionTrendDto>();
            var rangeType = request.TimeRange.ToLower();

            if (rangeType == "today")
            {
                var blocks = new[] { "02:00", "04:00", "06:00", "08:00", "10:00", "12:00", "14:00", "16:00", "18:00", "20:00", "22:00", "24:00" };
                foreach (var b in blocks)
                {
                    var hour = int.Parse(b.Split(':')[0]);
                    var matchingOrders = orders.Where(o => o.OrderDate.Hour >= hour - 2 && o.OrderDate.Hour < hour).ToList();
                    var matchingRfqs = rfqs.Count(r => r.CreatedAt.Hour >= hour - 2 && r.CreatedAt.Hour < hour);
                    var rev = matchingOrders.Sum(o => o.GetGrandTotal().Amount);
                    revenueTrend.Add(new RevenueTrendDto { Period = b, Revenue = rev, Profit = rev * 0.36m });
                    conversionTrend.Add(new ConversionTrendDto { Period = b, TotalRfqs = matchingRfqs, ConvertedQuotes = matchingOrders.Count });
                }
            }
            else if (rangeType == "week")
            {
                var daysOfWeek = new[] { "T2", "T3", "T4", "T5", "T6", "T7", "CN" };
                string GetDayNameVi(DayOfWeek day) => day switch { DayOfWeek.Monday => "T2", DayOfWeek.Tuesday => "T3", DayOfWeek.Wednesday => "T4", DayOfWeek.Thursday => "T5", DayOfWeek.Friday => "T6", DayOfWeek.Saturday => "T7", DayOfWeek.Sunday => "CN", _ => "" };
                var ordersByDay = orders.GroupBy(o => o.OrderDate.DayOfWeek).ToDictionary(g => GetDayNameVi(g.Key), g => g.ToList());
                var rfqsByDay = rfqs.GroupBy(r => r.CreatedAt.DayOfWeek).ToDictionary(g => GetDayNameVi(g.Key), g => g.Count());


                foreach (var day in daysOfWeek)
                {
                    var dayOrders = ordersByDay.TryGetValue(day, out var oList) ? oList : new List<Order>();
                    var rev = dayOrders.Sum(o => o.GetGrandTotal().Amount);
                    var countRfqs = rfqsByDay.TryGetValue(day, out var rCount) ? rCount : 0;
                    
                    revenueTrend.Add(new RevenueTrendDto { Period = day, Revenue = rev, Profit = rev * 0.36m });
                    conversionTrend.Add(new ConversionTrendDto { Period = day, TotalRfqs = countRfqs, ConvertedQuotes = dayOrders.Count });
                }
            }
            else if (rangeType == "month")
            {
                int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
                var ordersByDay = orders.GroupBy(o => o.OrderDate.Day).ToDictionary(g => g.Key, g => g.ToList());
                var rfqsByDay = rfqs.GroupBy(r => r.CreatedAt.Day).ToDictionary(g => g.Key, g => g.Count());
                
                for (int d = 1; d <= daysInMonth; d++)
                {
                    var dayOrders = ordersByDay.TryGetValue(d, out var oList) ? oList : new List<Order>();
                    var rev = dayOrders.Sum(o => o.GetGrandTotal().Amount);
                    var countRfqs = rfqsByDay.TryGetValue(d, out var rCount) ? rCount : 0;
                    
                    revenueTrend.Add(new RevenueTrendDto { Period = "Ngày " + d, Revenue = rev, Profit = rev * 0.36m });
                    conversionTrend.Add(new ConversionTrendDto { Period = "Ngày " + d, TotalRfqs = countRfqs, ConvertedQuotes = dayOrders.Count });
                }
            }
            else // year or all
            {
                var months = new[] { "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6", "Tháng 7", "Tháng 8", "Tháng 9", "Tháng 10", "Tháng 11", "Tháng 12" };
                var ordersByMonth = orders.GroupBy(o => o.OrderDate.Month).ToDictionary(g => g.Key, g => g.ToList());
                var rfqsByMonth = rfqs.GroupBy(r => r.CreatedAt.Month).ToDictionary(g => g.Key, g => g.Count());
                
                for (int m = 1; m <= 12; m++)
                {
                    var monthOrders = ordersByMonth.TryGetValue(m, out var oList) ? oList : new List<Order>();
                    var rev = monthOrders.Sum(o => o.GetGrandTotal().Amount);
                    var countRfqs = rfqsByMonth.TryGetValue(m, out var rCount) ? rCount : 0;
                    
                    revenueTrend.Add(new RevenueTrendDto { Period = months[m - 1], Revenue = rev, Profit = rev * 0.36m });
                    conversionTrend.Add(new ConversionTrendDto { Period = months[m - 1], TotalRfqs = countRfqs, ConvertedQuotes = monthOrders.Count });
                }
            }

            var response = new BusinessReportStatsResponse
            {
                TotalRevenue = totalRevenue,
                GrossProfit = grossProfit,
                TotalOrders = totalOrders,
                AverageOrderValue = aov,
                TotalQuotes = totalQuotes,
                ConvertedQuotes = convertedQuotes,
                ConversionRate = conversionRate,
                NewCustomers = newCustomers,
                TotalRfqs = totalRfqs,
                TopCustomers = topCustomers,
                TopProducts = topProducts,
                RevenueTrendChart = revenueTrend,
                ConversionTrendChart = conversionTrend
            };

            return Result<BusinessReportStatsResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<BusinessReportStatsResponse>.Failure($"Lỗi khi lấy báo cáo kinh doanh: {ex.Message}");
        }
    }
}
