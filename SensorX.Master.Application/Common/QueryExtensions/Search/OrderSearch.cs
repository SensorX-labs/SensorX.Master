using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

namespace SensorX.Master.Application.Common.QueryExtensions.Search;

public static class OrderSearch
{
    public static IQueryable<Order> ApplySearch(
        this IQueryable<Order> query,
        string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return query;

        var term = searchTerm.Trim().ToLower();

        return query.Where(o =>
            ((string)o.Code).ToLower().Contains(term) ||
            o.DeliveryInfo.CompanyName.ToLower().Contains(term) ||
            o.DeliveryInfo.RecipientName.ToLower().Contains(term) ||
            ((string)o.DeliveryInfo.RecipientPhone).ToLower().Contains(term) ||
            ((string)o.DeliveryInfo.Email).ToLower().Contains(term) ||
            o.DeliveryInfo.ShippingAddress.ToLower().Contains(term) ||
            o.SenderInfo.Name.ToLower().Contains(term));
    }
}
