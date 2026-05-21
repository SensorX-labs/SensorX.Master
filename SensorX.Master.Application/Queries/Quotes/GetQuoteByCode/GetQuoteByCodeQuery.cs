using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Quotes.GetQuoteByCode;

public record GetQuoteByCodeQuery(string Code) : IRequest<Result<GetQuoteByCodeResponse>>;

public record GetQuoteByCodeResponse(
    Guid Id,
    string Code,
    string Status,
    DateTimeOffset? QuoteDate,
    Guid CustomerId,
    string CompanyName,
    decimal GrandTotal,
    int ItemCount,
    DateTimeOffset CreatedAt
);
