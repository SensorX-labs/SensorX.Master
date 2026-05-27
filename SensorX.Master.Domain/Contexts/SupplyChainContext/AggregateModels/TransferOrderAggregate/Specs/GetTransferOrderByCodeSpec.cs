using Ardalis.Specification;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate.Specs;

public sealed class GetTransferOrderByCodeSpec : Specification<TransferOrder>
{
    public GetTransferOrderByCodeSpec(string code)
    {
        Query.Where(x => x.Code.Value == code);
    }
}
