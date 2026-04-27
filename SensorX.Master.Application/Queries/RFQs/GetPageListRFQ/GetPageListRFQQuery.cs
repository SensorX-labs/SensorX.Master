using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.RFQs.GetPageListRFQ;

public record GetPageListRFQQuery(
    string? SearchTerm,
    int PageNumber = 1,
    int PageSize = 10
) : OffsetPagedQuery(PageNumber, PageSize), IRequest<Result<OffsetPagedResult<GetPageListRFQResponse>>>;