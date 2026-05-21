using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Quotes.GetQuoteStats;

public sealed record GetQuoteStatsQuery() : IRequest<Result<QuoteStatsResponse>>;

public sealed record QuoteStatsResponse
{
    public int TotalCount { get; set; }
    public int DraftCount { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int ReturnedCount { get; set; }
    public int SentCount { get; set; }
    public int OrderedCount { get; set; }
    public int ExpiredCount { get; set; }
}