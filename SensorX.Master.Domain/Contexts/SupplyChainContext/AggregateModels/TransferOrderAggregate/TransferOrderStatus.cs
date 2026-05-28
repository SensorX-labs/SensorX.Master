namespace SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;

public enum TransferOrderStatus
{
    Processing, // đang xử lý 
    Delivering, // đang vận chuyển
    Completed // đã hoàn thành
}
