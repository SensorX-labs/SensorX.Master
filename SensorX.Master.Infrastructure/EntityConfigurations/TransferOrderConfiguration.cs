using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.TransferOrderAggregate;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Infrastructure.EntityConfigurations;

public class TransferOrderConfiguration : IEntityTypeConfiguration<TransferOrder>
{
    public void Configure(EntityTypeBuilder<TransferOrder> builder)
    {
        builder.ToTable("TransferOrders");

        builder.HasKey(to => to.Id);

        builder.Property(to => to.Id)
            .HasConversion(id => id.Value, v => new TransferOrderId(v))
            .ValueGeneratedNever();

        builder.Property(to => to.Code)
            .HasConversion(c => c.Value, v => Code.From(v));

        builder.Property(to => to.SourceWarehouseId)
            .HasConversion(id => id.Value, v => new WarehouseId(v));

        builder.Property(to => to.DestinationWarehouseId)
            .HasConversion(id => id.Value, v => new WarehouseId(v));

        builder.Property(to => to.SupplyRequestId)
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                v => v == null ? null : new SupplyRequestId(v.Value));

        builder.OwnsMany(to => to.Items, item =>
        {
            item.ToTable("TransferOrderItems");

            item.HasKey(i => i.Id);

            item.Property(i => i.Id)
                .HasConversion(id => id.Value, v => new TransferOrderItemId(v))
                .ValueGeneratedNever();

            item.Property(i => i.ProductId)
                .HasConversion(id => id.Value, v => new ProductId(v));

            item.Property(i => i.ProductCode)
                .HasConversion(c => c.Value, v => Code.From(v));

            item.Property(i => i.Quantity)
                .HasConversion(qty => qty.Value, v => new Quantity(v));
        });
        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(to => to.SourceWarehouseId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(to => to.DestinationWarehouseId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
