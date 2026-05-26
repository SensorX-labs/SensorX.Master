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
        var sourceQuery = _quoteQueryBuilder.QueryAsNoTracking;

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
        else
        {
            sourceQuery = sourceQuery.Where(x => x.Status != QuoteStatus.Draft);
        }

        sourceQuery = sourceQuery.ApplySearch(request.SearchTerm);

        if (request.Status is not null)
        {
            if (request.Status == QuoteStatus.Sent)
            {
                sourceQuery = sourceQuery.Where(x => x.Status == QuoteStatus.Ordered || x.Status == request.Status);
            }
            else
            {
                sourceQuery = sourceQuery.Where(x => x.Status == request.Status);
            }
        }

        if (request.ResponseType.HasValue)
        {
            sourceQuery = sourceQuery.Where(x => x.Response != null && x.Response.ResponseType == request.ResponseType.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = request.Code.Trim().ToLower();
            sourceQuery = sourceQuery.Where(x => ((string)x.Code).ToLower().Contains(code));
        }

        if (!string.IsNullOrWhiteSpace(request.CompanyName))
        {
            var companyName = request.CompanyName.Trim().ToLower();
            sourceQuery = sourceQuery.Where(x => x.CustomerInfo.CompanyName.ToLower().Contains(companyName));
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerEmail))
        {
            var customerEmail = request.CustomerEmail.Trim().ToLower();
            sourceQuery = sourceQuery.Where(x => ((string)x.CustomerInfo.Email).ToLower().Contains(customerEmail));
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerPhone))
        {
            var customerPhone = request.CustomerPhone.Trim().ToLower();
            sourceQuery = sourceQuery.Where(x => ((string)x.CustomerInfo.Phone).ToLower().Contains(customerPhone));
        }

        if (!string.IsNullOrWhiteSpace(request.SenderName))
        {
            var senderName = request.SenderName.Trim().ToLower();
            sourceQuery = sourceQuery.Where(x => x.SenderInfo.Name.ToLower().Contains(senderName));
        }

        if (request.QuoteDateFrom.HasValue)
        {
            sourceQuery = sourceQuery.Where(x => x.QuoteDate.HasValue && x.QuoteDate.Value >= request.QuoteDateFrom.Value);
        }

        if (request.QuoteDateTo.HasValue)
        {
            sourceQuery = sourceQuery.Where(x => x.QuoteDate.HasValue && x.QuoteDate.Value <= request.QuoteDateTo.Value);
        }

        if (request.CreatedFrom.HasValue)
        {
            sourceQuery = sourceQuery.Where(x => x.CreatedAt >= request.CreatedFrom.Value);
        }

        if (request.CreatedTo.HasValue)
        {
            sourceQuery = sourceQuery.Where(x => x.CreatedAt <= request.CreatedTo.Value);
        }

        if (request.IsExpired == true)
        {
            sourceQuery = sourceQuery.Where(x => 
                x.QuoteDate > DateTimeOffset.UtcNow && 
                x.Status != QuoteStatus.Ordered && 
                x.QuoteDate != null);
        }

        var hasTotalFilter = request.TotalFrom.HasValue || request.TotalTo.HasValue;

        if (hasTotalFilter)
        {
            var orderedQuotes = sourceQuery
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id);

            var materializedQuotes = await _queryExecutor.ToListAsync(orderedQuotes, cancellationToken);

            var filteredQuotes = materializedQuotes
                .Select(x => new
                {
                    Quote = x,
                    GrandTotal = x.GetGrandTotal().Amount
                });

            if (request.TotalFrom.HasValue)
            {
                filteredQuotes = filteredQuotes.Where(x => x.GrandTotal >= request.TotalFrom.Value);
            }

            if (request.TotalTo.HasValue)
            {
                filteredQuotes = filteredQuotes.Where(x => x.GrandTotal <= request.TotalTo.Value);
            }

            var filteredQuoteList = filteredQuotes.ToList();
            var totalCountWithAmount = filteredQuoteList.Count;

            var pagedItems = filteredQuoteList
                .Skip(((request.PageNumber ?? 1) - 1) * (request.PageSize ?? 10))
                .Take(request.PageSize ?? 10)
                .Select(x => new GetPageListQuoteResponse(
                    x.Quote.Id.Value,
                    x.Quote.Code.Value,
                    x.Quote.Status,
                    x.Quote.QuoteDate,
                    x.Quote.CustomerId,
                    x.Quote.CustomerInfo.CompanyName,
                    x.GrandTotal,
                    x.Quote.LineItems.Count,
                    x.Quote.CreatedAt,
                    x.Quote.Response != null ? x.Quote.Response.ResponseType : null
                ))
                .ToList();

            var resultWithAmount = new OffsetPagedResult<GetPageListQuoteResponse>
            {
                Items = pagedItems,
                PageNumber = request.PageNumber ?? 1,
                PageSize = request.PageSize ?? 10,
                TotalCount = totalCountWithAmount
            };

            return Result<OffsetPagedResult<GetPageListQuoteResponse>>.Success(resultWithAmount);
        }

        var totalCount = await _queryExecutor.CountAsync(sourceQuery, cancellationToken);

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
            x.CreatedAt,
            x.Response != null ? x.Response.ResponseType : null
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
