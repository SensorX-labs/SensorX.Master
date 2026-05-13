using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.LoadMore;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Queries.RFQs.GetMyRFQPage;

public sealed record GetMyRFQPageQuery(
    string? SearchTerm,
    RFQStatus? Status
) : LoadMoreQuery, IRequest<Result<GetMyRFQResult>>;


public sealed record GetMyRFQResponse(
    Guid Id,
    string Code,
    RFQStatus Status,
    DateTimeOffset CreatedAt
);

public sealed class GetMyRFQResult : LoadMoreResult<GetMyRFQResponse>;
