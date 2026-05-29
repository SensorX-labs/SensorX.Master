using Ardalis.Specification;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate.Specifications;

public class GetTransferOrderByCode : Specification<TransferOrder>, ISingleResultSpecification<TransferOrder>
{
    public GetTransferOrderByCode(Code code)
    {
        Query.Where(x => x.Code.Value == code.Value);
    }
}
