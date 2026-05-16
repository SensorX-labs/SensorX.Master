using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.RFQs.GetStats;

public sealed record GetStatsQuery : IRequest<Result<GetStatsResponse>>;

public sealed record GetStatsResponse(
    int Total,
    int Pending,
    int Accepted,
    int Rejected,
    int Responded,
    int Converted
);