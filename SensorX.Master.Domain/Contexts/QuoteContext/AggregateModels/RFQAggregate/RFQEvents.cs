using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

public sealed record RFQAllRejectedEvent(RFQId RfqId, Code Code) : IDomainEvent;
public sealed record RFQSendedEvent(RFQId RfqId, Code Code) : IDomainEvent;
public sealed record RFQAssignedEvent(RFQId RfqId, Code Code, StaffId StaffId) : IDomainEvent;
public sealed record RFQAceptedEvent(RFQId RfqId, Code Code, StaffId StaffId) : IDomainEvent;
public sealed record RFQRejectedEvent(RFQId RfqId, Code Code, StaffId StaffId) : IDomainEvent;
public sealed record RFQForceAssignedEvent(RFQId RfqId, Code Code, StaffId StaffId) : IDomainEvent;
public sealed record RFQMarkAsRespondedEvent(RFQId RfqId, Code Code) : IDomainEvent;
public sealed record RFQMarkAsConvertedEvent(RFQId RfqId, Code Code) : IDomainEvent;
