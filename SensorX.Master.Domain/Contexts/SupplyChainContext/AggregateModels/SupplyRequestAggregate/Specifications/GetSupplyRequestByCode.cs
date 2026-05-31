using Ardalis.Specification;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate.Specifications;

public sealed class GetSupplyRequestByCode : Specification<SupplyRequest>, ISingleResultSpecification<SupplyRequest>
{
    public GetSupplyRequestByCode(Code code)
    {
        Query.Where(x => x.Code == code);
    }
}
