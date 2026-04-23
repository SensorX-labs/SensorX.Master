using Bogus;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace SensorX.Master.Infrastructure.Persistences;

public static class BogusSeeder
{
    public static async Task SeedData(AppDbContext context)
    {
        await SeedRFQs(context);
        await SeedQuotes(context);
        await SeedOrders(context);
    }

    private static async Task SeedRFQs(AppDbContext context)
    {
        if (await context.Set<RFQ>().AnyAsync())
        {
            return;
        }

        var rfqFaker = new Faker<RFQ>("vi")
            .CustomInstantiator(f =>
            {
                var id = RFQId.New();
                var customerId = CustomerId.New();
                var staffId = StaffId.New();
                var code = Code.Create("RFQ");
                var description = f.Lorem.Sentence();
                var customerInfo = new CustomerInfo(f.Name.FullName(), f.Phone.PhoneNumber("0#########"), f.Address.FullAddress(), f.Company.CompanyName(), f.Random.Replace("#########"));
                
                var rfq = RFQ.Create(id, customerId, staffId, code, description, customerInfo);
                
                for (int i = 0; i < f.Random.Number(1, 5); i++)
                {
                    rfq.AddItem(ProductId.New(), f.Commerce.ProductName(), Quantity.Create(f.Random.Number(1, 100)), f.Lorem.Word());
                }
                
                return rfq;
            });

        var rfqs = rfqFaker.Generate(30);
        await context.Set<RFQ>().AddRangeAsync(rfqs);
        await context.SaveChangesAsync();
    }

    private static async Task SeedQuotes(AppDbContext context)
    {
        if (await context.Set<Quote>().AnyAsync())
        {
            return;
        }

        var rfqs = await context.Set<RFQ>().Take(10).ToListAsync();
        
        var quoteFaker = new Faker<Quote>("vi")
            .CustomInstantiator(f =>
            {
                var rfq = f.PickRandom(rfqs);
                var id = QuoteId.New();
                var code = Code.Create("QT");
                var quoteDate = f.Date.PastOffset(1).ToUniversalTime();
                
                var quote = new Quote(
                    id,
                    code,
                    rfq.Id,
                    rfq.CustomerId,
                    rfq.CustomerInfo,
                    f.Lorem.Sentence(),
                    f.PickRandom<QuoteStatus>(),
                    null,
                    quoteDate,
                    ""
                );

                foreach(var item in rfq.Items)
                {
                    var price = Money.Create(f.Random.Decimal(100000, 1000000), "VND");
                    quote.AddItem(new QuoteItem(QuoteItemId.New(), item.ProductId, item.ProductName, item.Quantity, price, Percent.Create(10)));
                }

                return quote;
            });

        var quotes = quoteFaker.Generate(20);
        await context.Set<Quote>().AddRangeAsync(quotes);
        await context.SaveChangesAsync();
    }

    private static async Task SeedOrders(AppDbContext context)
    {
        if (await context.Set<Order>().AnyAsync())
        {
            return;
        }

        var quotes = await context.Set<Quote>().Where(q => q.Status == QuoteStatus.Approved || q.Status == QuoteStatus.Sent).Take(10).ToListAsync();
        if (!quotes.Any()) return;

        var orderFaker = new Faker<Order>("vi")
            .CustomInstantiator(f =>
            {
                var quote = f.PickRandom(quotes);
                var id = OrderId.New();
                var code = Code.Create("ORD");
                var orderDate = f.Date.PastOffset(1).ToUniversalTime();
                var senderInfo = new SenderInfo("SensorX HQ", "0241234567", "Hanoi, Vietnam");

                var order = new Order(
                    id,
                    quote.Id,
                    code,
                    quote.CustomerId,
                    quote.CustomerInfo,
                    senderInfo,
                    f.PickRandom<OrderStatus>(),
                    orderDate
                );

                foreach(var item in quote.LineItems)
                {
                    order.AddItem(new OrderItem(OrderItemId.New(), item.ProductId, item.ProductName, item.Quantity, item.Price, item.TaxPercent));
                }

                return order;
            });

        var orders = orderFaker.Generate(15);
        await context.Set<Order>().AddRangeAsync(orders);
        await context.SaveChangesAsync();
    }
}
