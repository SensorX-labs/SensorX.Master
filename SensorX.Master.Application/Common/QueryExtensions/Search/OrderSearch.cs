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
            o.Code.Value.ToLower().Contains(term) ||
            (o.DeliveryInfo.CompanyName != null && o.DeliveryInfo.CompanyName.ToLower().Contains(term)) ||
            (o.DeliveryInfo.RecipientName != null && o.DeliveryInfo.RecipientName.ToLower().Contains(term)) ||
            (o.DeliveryInfo.RecipientPhone != null && o.DeliveryInfo.RecipientPhone.Value.ToLower().Contains(term)) ||
            (o.DeliveryInfo.Email != null && o.DeliveryInfo.Email.Value.ToLower().Contains(term)) ||
            (o.DeliveryInfo.ShippingAddress != null && o.DeliveryInfo.ShippingAddress.ToLower().Contains(term)) ||
            (o.SenderInfo.Name != null && o.SenderInfo.Name.ToLower().Contains(term)));
    }
}
