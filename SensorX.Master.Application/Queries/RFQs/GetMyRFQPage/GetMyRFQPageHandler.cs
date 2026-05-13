using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Queries.RFQs.GetMyRFQPage;

public class GetMyRFQPageHandler(
    IQueryBuilder<RFQ> _rfqBuilder,
    ICurrentUser _currentUser
) : IRequestHandler<GetMyRFQPageQuery, GetMyRFQResult>
{
    public async Task<GetMyRFQResult> Handle(GetMyRFQPageQuery request, CancellationToken cancellationToken)
    {
        var result = new GetMyRFQResult();
        return result;
    }
}