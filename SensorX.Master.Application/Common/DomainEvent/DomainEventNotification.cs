using SensorX.Master.Domain.SeedWork;
using MediatR;

namespace SensorX.Master.Application.Common.DomainEvent;
public record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent): INotification where TDomainEvent : IDomainEvent;

