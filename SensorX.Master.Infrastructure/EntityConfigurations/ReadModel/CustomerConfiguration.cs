namespace SensorX.Master.Infrastructure.EntityConfigurations.ReadModel;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Application.Common.ReadModel;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("CustomerSnapshots", "read");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, v => new Domain.StrongIDs.CustomerId(v))
            .ValueGeneratedNever();

        builder.Property(x => x.AccountId)
            .HasConversion(id => id.Value, v => new Domain.StrongIDs.AccountId(v));

        builder.Property(x => x.Phone)
            .HasConversion(p => p != null ? p.Value : null, v => v != null ? Domain.ValueObjects.Phone.From(v) : null);

        builder.Property(x => x.RecipientPhone)
            .HasConversion(p => p != null ? p.Value : null, v => v != null ? Domain.ValueObjects.Phone.From(v) : null);

        builder.Property(x => x.Email)
            .HasConversion(e => e.Value, v => Domain.ValueObjects.Email.From(v));
    }
}