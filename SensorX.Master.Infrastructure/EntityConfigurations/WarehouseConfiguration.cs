using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.WarehouseAggregate;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Infrastructure.EntityConfigurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasConversion(id => id.Value, v => new WarehouseId(v))
            .ValueGeneratedNever();
    }
}
