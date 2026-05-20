using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Queries.Quotes.GetQuoteByCode;

public class GetQuoteByCodeHandler(
    IQueryBuilder<Quote> _quoteQueryBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetQuoteByCodeQuery, Result<GetQuoteByCodeResponse>>
{
    public async Task<Result<GetQuoteByCodeResponse>> Handle(GetQuoteByCodeQuery request, CancellationToken cancellationToken)
    {
        var codeValue = request.Code.Trim();
        var targetCode = Code.From(codeValue);
        var query = _quoteQueryBuilder.QueryAsNoTracking
            .Where(x => x.Code == targetCode)
            .Select(x => new GetQuoteByCodeResponse(
                x.Id.Value,
                x.Code.Value,
                x.Status.ToString(),
                x.QuoteDate,
                x.CustomerId.Value,
                x.CustomerInfo.CompanyName,
                x.GetGrandTotal().Amount,
                x.LineItems.Count,
                x.CreatedAt
            ));

        var quote = await _queryExecutor.FirstOrDefaultAsync(query, cancellationToken);

        if (quote == null)
        {
            return Result<GetQuoteByCodeResponse>.Failure($"Không tìm thấy báo giá với mã {request.Code}");
        }

        return Result<GetQuoteByCodeResponse>.Success(quote);
    }
}
