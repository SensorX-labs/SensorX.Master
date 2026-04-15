using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Infrastructure.EntityConfigurations;

public class SupplyRequestConfiguration : IEntityTypeConfiguration<SupplyRequest>
{
    public void Configure(EntityTypeBuilder<SupplyRequest> builder)
    {
        builder.ToTable("SupplyRequests");

        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.Id)
            .HasConversion(id => id.Value, v => new SupplyRequestId(v))
            .ValueGeneratedNever();

        builder.Property(sr => sr.WarehouseId)
            .HasConversion(id => id.Value, v => new WarehouseId(v));

        builder.OwnsMany(sr => sr.Items, item =>
        {
            item.ToTable("SupplyRequestItems");

            item.HasKey(i => i.Id);

            item.Property(i => i.Id)
                .HasConversion(id => id.Value, v => new SupplyRequestItemId(v))
                .ValueGeneratedNever();

            item.Property(i => i.ProductId)
                .HasConversion(id => id.Value, v => new ProductId(v));

            item.Property(i => i.RequestedQuantity)
                .HasConversion(qty => qty.Value, v => new Quantity(v));
        });

        builder.OwnsMany(sr => sr.PurchaseOptions, po =>
        {
            po.ToTable("PurchaseOptions");

            po.HasKey(p => p.Id);

            po.Property(p => p.Id)
                .HasConversion(id => id.Value, v => new PurchaseOptionId(v))
                .ValueGeneratedNever();

            po.Property(p => p.ProductId)
                .HasConversion(id => id.Value, v => new ProductId(v));

            po.Property(p => p.Quantity)
                .HasConversion(qty => qty.Value, v => new Quantity(v));
        });
    }
}
