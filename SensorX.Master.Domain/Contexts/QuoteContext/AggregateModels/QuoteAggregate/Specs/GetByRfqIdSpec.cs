using Ardalis.Specification;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.Specs;

public sealed class GetByRfqIdSpec : Specification<Quote>
{
    public GetByRfqIdSpec(QuoteId id, RFQId rfqId)
    {
        Query.Where(x => x.Id != id && x.Status != QuoteStatus.Sent);
        Query.Where(x => x.RFQId == rfqId);
    }
}