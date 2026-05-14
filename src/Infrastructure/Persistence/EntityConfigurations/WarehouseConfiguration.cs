using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Domain.AggregatesModel;

namespace SensorX.Master.Infrastructure.Persistence.EntityConfigurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Id)
            .HasConversion(id => id.Value, v => new Domain.ValueObjects.WarehouseId(v))
            .ValueGeneratedNever();

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Address)
            .HasMaxLength(500);

        builder.Property(w => w.ApiEndpointUrl)
            .HasConversion(url => url.Value, v => new Domain.ValueObjects.ApiEndpointUrl(v))
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(w => w.IsActive)
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .IsRequired();

        builder.Property(w => w.UpdatedAt);

        builder.HasIndex(w => w.ApiEndpointUrl).IsUnique();
    }
}
