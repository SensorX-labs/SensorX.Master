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
            x.CustomerInfo.RecipientName,
            x.CustomerInfo.CompanyName,
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
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<OffsetPagedResult<GetPageListOrderResponse>>.Success(result);
    }
}
