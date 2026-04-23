using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Quotes.GetPageListQuote;

public record GetPageListQuoteQuery(
    string? SearchTerm
) : OffsetPagedQuery, IRequest<Result<QuoteOffsetPagedResult>>;
