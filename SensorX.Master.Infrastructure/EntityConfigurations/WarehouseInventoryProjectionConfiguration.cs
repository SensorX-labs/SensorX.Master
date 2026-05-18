using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Domain.Contexts.SupplyChainContext.ReadModels;

namespace SensorX.Master.Infrastructure.EntityConfigurations;

public class WarehouseInventoryProjectionConfiguration : IEntityTypeConfiguration<WarehouseInventoryProjection>
{
    public void Configure(EntityTypeBuilder<WarehouseInventoryProjection> builder)
    {
        builder.ToTable("WarehouseInventoryProjections");

        builder.HasKey(x => new { x.WarehouseId, x.ProductId });

        builder.Property(x => x.ProductCode).HasMaxLength(100);
        builder.Property(x => x.ProductName).HasMaxLength(500);
        builder.Property(x => x.Unit).HasMaxLength(50);
        builder.Property(x => x.WarehouseName).HasMaxLength(200);
        builder.Property(x => x.BrandZone).HasMaxLength(200);
        builder.Property(x => x.RackCode).HasMaxLength(200);
        builder.Property(x => x.LastSyncAt).IsRequired();
    }
}
