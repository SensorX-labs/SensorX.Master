using MediatR;
using SensorX.Master.Application.Common.Pagination;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.RFQs.GetPageListRFQ;

public record GetPageListRFQQuery(
    string? SearchTerm
) : CursorPagedQuery, IRequest<Result<RFQCursorPagedResult>>;