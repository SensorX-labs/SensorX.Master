using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Queries.RFQs.GetRFQById;

public class GetRFQByIdQuery : IRequest<Result<GetRFQByIdResponse>>
{
    public Guid RFQId { get; set; }
}