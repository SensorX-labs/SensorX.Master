using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Tests.Contexts.SupplyChainContext;

public class SupplyChainContextTests
{
    // ─── SupplyRequest ───────────────────────────────────────────────
    [Fact]
    public void SupplyRequest_AddItem_ShouldIncreaseItemCount()
    {
        // Arrange
        var request = new SupplyRequest(
            SupplyRequestId.New(),
            WarehouseId.New(),
            SupplyRequestStatus.Pending,
            "Test note"
        );

        // Act
        request.AddItem(new ProductId(Guid.NewGuid()), new Quantity(10));
        request.AddItem(new ProductId(Guid.NewGuid()), new Quantity(5));

        // Assert
        Assert.Equal(2, request.Items.Count);
        Assert.Equal(10, request.Items[0].RequestedQuantity.Value);
        Assert.Equal(5, request.Items[1].RequestedQuantity.Value);
    }

    [Fact]
    public void SupplyRequest_Complete_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var request = new SupplyRequest(
            SupplyRequestId.New(),
            WarehouseId.New(),
            SupplyRequestStatus.Pending,
            "Test note"
        );

        // Act
        request.Complete();

        // Assert
        Assert.Equal(SupplyRequestStatus.Completed, request.Status);
    }

    [Fact]
    public void SupplyRequest_AddPurchaseOption_ShouldIncreaseOptionCount()
    {
        // Arrange
        var request = new SupplyRequest(
            SupplyRequestId.New(),
            WarehouseId.New(),
            SupplyRequestStatus.Pending,
            "Test note"
        );

        // Act
        request.AddPurchaseOption(
            new ProductId(Guid.NewGuid()),
            new Quantity(3),
            "Ghi chú"
        );

        // Assert
        Assert.Single(request.PurchaseOptions);
    }

    // ─── TransferOrder ───────────────────────────────────────────────
    [Fact]
    public void TransferOrder_AddItem_ShouldIncreaseItemCount()
    {
        // Arrange
        var transfer = new TransferOrder(
            TransferOrderId.New(),
            Code.Create("TO"),
            WarehouseId.New(),
            WarehouseId.New(),
            TransferOrderStatus.Processing,
            "Test note"
        );

        // Act
        transfer.AddItem(
            new ProductId(Guid.NewGuid()),
            Code.Create("PRD"),
            "Sản phẩm B",
            "Hộp",
            new Quantity(20),
            "Nhà sản xuất Y",
            "Không"
        );

        // Assert
        Assert.Single(transfer.Items);
        Assert.Equal(20, transfer.Items[0].Quantity.Value);
    }

    [Fact]
    public void TransferOrder_Complete_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var transfer = new TransferOrder(
            TransferOrderId.New(),
            Code.Create("TO"),
            WarehouseId.New(),
            WarehouseId.New(),
            TransferOrderStatus.Processing,
            "Test note"
        );

        // Act
        transfer.Complete();

        // Assert
        Assert.Equal(TransferOrderStatus.Completed, transfer.Status);
    }

    [Fact]
    public void TransferOrder_LinkedToSupplyRequest_ShouldHaveSupplyRequestId()
    {
        // Arrange
        var supplyRequestId = SupplyRequestId.New();
        var transfer = new TransferOrder(
            TransferOrderId.New(),
            Code.Create("TO"),
            WarehouseId.New(),
            WarehouseId.New(),
            TransferOrderStatus.Processing,
            "Test note",
            supplyRequestId
        );

        // Assert
        Assert.Equal(supplyRequestId, transfer.SupplyRequestId);
    }
}
