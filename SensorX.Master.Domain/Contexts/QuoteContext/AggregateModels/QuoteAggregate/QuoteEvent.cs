using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;

public sealed record QuoteCreatedEvent(QuoteId QuoteId, RFQId RFQId) : IDomainEvent;
public sealed record CustomerRespondedQuoteEvent(QuoteId QuoteId, RFQId RFQId, QuoteResponse QuoteResponse) : IDomainEvent;
public sealed record PublishQuoteEvent(QuoteId QuoteId, RFQId RFQId, StaffId StaffId) : IDomainEvent;