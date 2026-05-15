using MediatR;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.UseCases.Orders.Queries.GetOrders;

public record GetOrdersQuery : IRequest<List<OrderDto>>;

public record OrderDto(
    Guid Id,
    string Code,
    Guid CustomerId,
    string RecipientName,
    string Status,
    DateTimeOffset OrderDate
);

public class GetOrdersQueryHandler(IRepository<Order> orderRepository) : IRequestHandler<GetOrdersQuery, List<OrderDto>>
{
    public async Task<List<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await orderRepository.ListAsync(cancellationToken);

        return orders.Select(o => new OrderDto(
            o.Id.Value,
            o.Code.Value,
            o.CustomerId.Value,
            o.CustomerInfo.RecipientName,
            o.Status.ToString(),
            o.OrderDate
        )).ToList();
    }
}