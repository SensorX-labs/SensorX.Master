using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Queries.RFQs.GetPageListRFQ;

public sealed record GetPageListRFQQuery : OffsetPagedQuery, IRequest<Result<GetPageListRFQResult>>
{
    public string? SearchTerm { get; init; }
    public RFQStatus? Status { get; init; }
}

public sealed record GetPageListRFQResponse
(
    Guid Id,
    string Code,
    string Status,
    string RecipientName,
    string RecipientPhone,
    string CompanyName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    Guid? StaffId,
    string? StaffName,
    int ItemCount
);
public sealed class GetPageListRFQResult : OffsetPagedResult<GetPageListRFQResponse>;
