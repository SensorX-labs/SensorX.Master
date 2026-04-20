using MediatR;
using SensorX.Master.Application.Common.Pagination;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Quotes.GetPageListQuote;

public record GetPageListQuoteQuery(
    string? SearchTerm
) : CursorPagedQuery, IRequest<Result<QuoteCursorPagedResult>>;
