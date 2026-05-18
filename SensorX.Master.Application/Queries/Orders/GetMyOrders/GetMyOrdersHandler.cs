using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.QueryExtensions.Search;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.Queries.Orders.GetPageListOrder;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Queries.Orders.GetMyOrders;

public class GetMyOrdersHandler(
    IQueryBuilder<Order> orderQueryBuilder,
    IQueryExecutor queryExecutor,
    ICurrentUser currentUser,
    IDataServiceClient dataServiceClient
) : IRequestHandler<GetMyOrdersQuery, Result<OffsetPagedResult<GetPageListOrderResponse>>>
{
    public async Task<Result<OffsetPagedResult<GetPageListOrderResponse>>> Handle(
        GetMyOrdersQuery request,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
            return Result<OffsetPagedResult<GetPageListOrderResponse>>.Failure("Nguoi dung chua duoc xac thuc");

        var customerResponse = await dataServiceClient.GetCustomerByAccountIdAsync(currentUser.UserId.Value);
        if (!customerResponse.IsSuccess || customerResponse.Value is null)
            return Result<OffsetPagedResult<GetPageListOrderResponse>>.Failure(customerResponse.Message ?? "Khong tim thay customer cua nguoi dung hien tai");

        var customerId = new CustomerId(customerResponse.Value.Id);
        var sourceQuery = orderQueryBuilder.QueryAsNoTracking
            .Where(x => x.CustomerId == customerId)
            .ApplySearch(request.SearchTerm);

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

        return Result<OffsetPagedResult<GetPageListOrderResponse>>.Success(new OffsetPagedResult<GetPageListOrderResponse>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        });
    }
}
