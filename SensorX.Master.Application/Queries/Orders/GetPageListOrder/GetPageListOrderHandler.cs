using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.QueryExtensions.Search;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;

namespace SensorX.Master.Application.Queries.Orders.GetPageListOrder;

public class GetPageListOrderHandler(
    IQueryBuilder<Order> orderQueryBuilder,
    IQueryExecutor queryExecutor
) : IRequestHandler<GetPageListOrderQuery, Result<OffsetPagedResult<GetPageListOrderResponse>>>
{
    public async Task<Result<OffsetPagedResult<GetPageListOrderResponse>>> Handle(
        GetPageListOrderQuery request,
        CancellationToken cancellationToken)
    {
        var sourceQuery = orderQueryBuilder.QueryAsNoTracking.ApplySearch(request.SearchTerm);
        if (!string.IsNullOrWhiteSpace(request.Status)
            && Enum.TryParse<OrderStatus>(request.Status, true, out var status))
        {
            sourceQuery = sourceQuery.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim().ToLower();
            sourceQuery = sourceQuery.Where(x => x.Code.Value.ToLower().Contains(code));
        }

        if (!string.IsNullOrWhiteSpace(request.CompanyName))
        {
            var companyName = request.CompanyName.Trim().ToLower();
            sourceQuery = sourceQuery.Where(x => x.DeliveryInfo.CompanyName.ToLower().Contains(companyName));
        }

        if (!string.IsNullOrWhiteSpace(request.RecipientName))
        {
            var recipientName = request.RecipientName.Trim().ToLower();
            sourceQuery = sourceQuery.Where(x => x.DeliveryInfo.RecipientName.ToLower().Contains(recipientName));
        }

        if (!string.IsNullOrWhiteSpace(request.RecipientPhone))
        {
            var recipientPhone = request.RecipientPhone.Trim().ToLower();
            sourceQuery = sourceQuery.Where(x => x.DeliveryInfo.RecipientPhone.Value.ToLower().Contains(recipientPhone));
        }

        if (!string.IsNullOrWhiteSpace(request.SenderName))
        {
            var senderName = request.SenderName.Trim().ToLower();
            sourceQuery = sourceQuery.Where(x => x.SenderInfo.Name.ToLower().Contains(senderName));
        }

        if (request.OrderDateFrom.HasValue)
        {
            sourceQuery = sourceQuery.Where(x => x.OrderDate >= request.OrderDateFrom.Value);
        }

        if (request.OrderDateTo.HasValue)
        {
            sourceQuery = sourceQuery.Where(x => x.OrderDate <= request.OrderDateTo.Value);
        }

        if (request.CreatedFrom.HasValue)
        {
            sourceQuery = sourceQuery.Where(x => x.CreatedAt >= request.CreatedFrom.Value);
        }

        if (request.CreatedTo.HasValue)
        {
            sourceQuery = sourceQuery.Where(x => x.CreatedAt <= request.CreatedTo.Value);
        }

        var hasTotalFilter = request.TotalFrom.HasValue || request.TotalTo.HasValue;

        if (hasTotalFilter)
        {
            var orderedOrders = sourceQuery
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.OrderDate);

            var materializedOrders = await queryExecutor.ToListAsync(orderedOrders, cancellationToken);

            var filteredOrders = materializedOrders.Select(x => new
            {
                Order = x,
                GrandTotal = x.GetGrandTotal().Amount
            });

            if (request.TotalFrom.HasValue)
            {
                filteredOrders = filteredOrders.Where(x => x.GrandTotal >= request.TotalFrom.Value);
            }

            if (request.TotalTo.HasValue)
            {
                filteredOrders = filteredOrders.Where(x => x.GrandTotal <= request.TotalTo.Value);
            }

            var filteredOrderList = filteredOrders.ToList();
            var totalCountWithAmount = filteredOrderList.Count;
            var pageNumber = request.PageNumber ?? 1;
            var pageSize = request.PageSize ?? 10;

            var pagedItems = filteredOrderList
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new GetPageListOrderResponse(
                    x.Order.Id.Value,
                    x.Order.QuoteId.Value,
                    x.Order.Code.Value,
                    x.Order.CustomerId.Value,
                    x.Order.DeliveryInfo.RecipientName,
                    x.Order.DeliveryInfo.CompanyName,
                    x.Order.Status.ToString(),
                    x.Order.OrderDate,
                    x.GrandTotal,
                    x.Order.Items.Count,
                    x.Order.CreatedAt
                )).ToList();

            var resultWithAmount = new OffsetPagedResult<GetPageListOrderResponse>
            {
                Items = pagedItems,
                TotalCount = totalCountWithAmount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return Result<OffsetPagedResult<GetPageListOrderResponse>>.Success(resultWithAmount);
        }

        var totalCount = await queryExecutor.CountAsync(sourceQuery, cancellationToken);

        var pagedQuery = sourceQuery
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.OrderDate)
            .ApplyOffsetPagination(request);

        var orders = await queryExecutor.ToListAsync(pagedQuery, cancellationToken);

        var items = orders.Select(x => new GetPageListOrderResponse(
            x.Id.Value,
            x.QuoteId.Value,
            x.Code.Value,
            x.CustomerId.Value,
            x.DeliveryInfo.RecipientName,
            x.DeliveryInfo.CompanyName,
            x.Status.ToString(),
            x.OrderDate,
            x.GetGrandTotal().Amount,
            x.Items.Count,
            x.CreatedAt
        )).ToList();

        var result = new OffsetPagedResult<GetPageListOrderResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber ?? 1,
            PageSize = request.PageSize ?? 10
        };

        return Result<OffsetPagedResult<GetPageListOrderResponse>>.Success(result);
    }
}
