using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.QueryExtensions.Search;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

namespace SensorX.Master.Application.Queries.Quotes.GetPageListQuote;

public class GetPageListQuoteHandler(
    IQueryBuilder<Quote> _quoteQueryBuilder,
    IQueryBuilder<SaleStaff> _saleStaffBulder,
    IQueryExecutor _queryExecutor,
    ICurrentUser _currentUser
) : IRequestHandler<GetPageListQuoteQuery, Result<OffsetPagedResult<GetPageListQuoteResponse>>>
{
    public async Task<Result<OffsetPagedResult<GetPageListQuoteResponse>>> Handle(
        GetPageListQuoteQuery request,
        CancellationToken cancellationToken)
    {
        var sourceQuery = _quoteQueryBuilder.QueryAsNoTracking.ApplySearch(request.SearchTerm);
        var totalCount = await _queryExecutor.CountAsync(sourceQuery, cancellationToken);

        if (_currentUser.Role == Role.SaleStaff)
        {
            var staffId = await _queryExecutor.FirstOrDefaultAsync(
                _saleStaffBulder.QueryAsNoTracking
                    .Where(x => x.AccountId == _currentUser.UserId)
                    .Select(x => x.Id),
                cancellationToken
            );

            sourceQuery = sourceQuery.Where(x => x.SenderInfo.Id == staffId);
        }

        if (request.Status is not null)
        {
            sourceQuery = sourceQuery.Where(x => x.Status == request.Status);
        }

        var pagedQuery = sourceQuery
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .ApplyOffsetPagination(request);

        var dtoQuery = pagedQuery.Select(x => new GetPageListQuoteResponse(
            x.Id.Value,
            x.Code.Value,
            x.Status,
            x.QuoteDate,
            x.CustomerId,
            x.CustomerInfo.CompanyName,
            x.GetGrandTotal().Amount,
            x.LineItems.Count,
            x.CreatedAt
        ));

        var items = await _queryExecutor.ToListAsync(dtoQuery, cancellationToken);

        var result = new OffsetPagedResult<GetPageListQuoteResponse>
        {
            Items = items,
            PageNumber = request.PageNumber ?? 1,
            PageSize = request.PageSize ?? 10,
            TotalCount = totalCount
        };

        return Result<OffsetPagedResult<GetPageListQuoteResponse>>.Success(result);
    }
}
