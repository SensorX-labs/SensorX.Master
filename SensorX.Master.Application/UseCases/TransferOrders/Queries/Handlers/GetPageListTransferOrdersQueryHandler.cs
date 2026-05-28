using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.UseCases.TransferOrders.Queries.Handlers;

public class GetPageListTransferOrdersQueryHandler(
    IRepository<TransferOrder> transferOrderRepository
) : IRequestHandler<GetPageListTransferOrdersQuery, Result<object>>
{
    public async Task<Result<object>> Handle(GetPageListTransferOrdersQuery request, CancellationToken cancellationToken)
    {
        var allTransferOrders = await transferOrderRepository.ListAsync(cancellationToken);

        var totalCount = allTransferOrders.Count;

        var pagedOrders = allTransferOrders
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var itemsDto = pagedOrders.Select(to => new TransferOrderListItemDto(
            to.Id.Value,
            to.Code.Value,
            to.SourceWarehouseId.Value,
            to.DestinationWarehouseId.Value,
            to.Status.ToString(),
            to.Note ?? "",
            to.CreatedAt
        )).ToList();

        return Result<object>.Success(new { items = itemsDto, totalCount });
    }
}
