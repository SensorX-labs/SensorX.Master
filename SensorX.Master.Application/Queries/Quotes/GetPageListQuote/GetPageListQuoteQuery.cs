using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Quotes.GetPageListQuote;

public record GetPageListQuoteQuery(
    string? SearchTerm,
    int PageNumber = 1,
    int PageSize = 10
) : OffsetPagedQuery(PageNumber, PageSize), IRequest<Result<OffsetPagedResult<GetPageListQuoteResponse>>>;
