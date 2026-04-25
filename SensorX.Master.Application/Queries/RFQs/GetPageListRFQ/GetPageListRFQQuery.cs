using MediatR;
using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.RFQs.GetPageListRFQ;

public record GetPageListRFQQuery(
    string? SearchTerm
) : OffsetPagedQuery, IRequest<Result<RFQOffsetPagedResult>>;