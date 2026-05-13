using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.UseCases.SupplyRequests.Queries.Handlers;

public class GetPageListSupplyRequestsQueryHandler(
    IRepository<SupplyRequest> supplyRequestRepository
) : IRequestHandler<GetPageListSupplyRequestsQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetPageListSupplyRequestsQuery request, CancellationToken cancellationToken)
    {
        var allRequests = await supplyRequestRepository.ListAsync(cancellationToken);

        var totalCount = allRequests.Count;

        var pagedRequests = allRequests
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var itemsDto = pagedRequests.Select(sr => new SupplyRequestListItemDto(
            sr.Id.Value,
            sr.Code.Value,
            sr.WarehouseId.Value,
            sr.Status.ToString(),
            sr.Note ?? "",
            sr.CreatedAt,
            sr.Items.Sum(i => i.RequestedQuantity.Value)
        )).ToList();

        return Result<object>.Success(new { items = itemsDto, totalCount });
    }
}
