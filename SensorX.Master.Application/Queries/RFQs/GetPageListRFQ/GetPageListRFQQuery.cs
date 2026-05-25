using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Queries.RFQs.GetPageListRFQ;

public sealed record GetPageListRFQQuery : OffsetPagedQuery, IRequest<Result<GetPageListRFQResult>>
{
    public string? SearchTerm { get; init; }
    public string? Code { get; init; }
    public string? CompanyName { get; init; }
    public string? RecipientName { get; init; }
    public string? RecipientPhone { get; init; }
    public string? StaffName { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
    public RFQStatus? Status { get; init; }
}

public sealed record GetPageListRFQResponse
(
    Guid Id,
    string Code,
    string Status,
    string CompanyName,
    string Phone,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    Guid? StaffId,
    string? StaffName,
    int ItemCount
);
public sealed class GetPageListRFQResult : OffsetPagedResult<GetPageListRFQResponse>;
