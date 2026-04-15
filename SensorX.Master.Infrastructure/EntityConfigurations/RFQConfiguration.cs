using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Infrastructure.EntityConfigurations;

public class RFQConfiguration : IEntityTypeConfiguration<RFQ>
{
    public void Configure(EntityTypeBuilder<RFQ> builder)
    {
        builder.ToTable("RFQs");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, v => new RFQId(v))
            .ValueGeneratedNever();

        builder.Property(r => r.Code)
            .HasConversion(c => c.Value, v => Code.From(v));

        builder.Property(r => r.StaffId)
            .HasConversion(id => id.Value, v => new StaffId(v));

        builder.Property(r => r.CustomerId)
            .HasConversion(id => id.Value, v => new CustomerId(v));

        builder.OwnsOne(r => r.CustomerInfo, c =>
        {
            c.Property(p => p.RecipientName).HasColumnName("RecipientName");
            c.Property(p => p.RecipientPhone)
                .HasConversion(p => p.Value, v => Phone.From(v))
                .HasColumnName("RecipientPhone");
            c.Property(p => p.CompanyName).HasColumnName("CompanyName");
            c.Property(p => p.Email).HasConversion(e => e.Value, v => Email.From(v)).HasColumnName("Email");
            c.Property(p => p.Address).HasColumnName("Address");
            c.Property(p => p.TaxCode).HasColumnName("TaxCode");
        });

        builder.OwnsMany(r => r.Items, item =>
        {
            item.ToTable("RFQItems");

            item.HasKey(i => i.Id);

            item.Property(i => i.Id)
                .HasConversion(id => id.Value, v => new RFQItemId(v))
                .ValueGeneratedNever();

            item.Property(i => i.ProductId)
                .HasConversion(id => id.Value, v => new ProductId(v));

            item.Property(i => i.ProductCode)
                .HasConversion(c => c.Value, v => Code.From(v));

            item.Property(i => i.Quantity)
                .HasConversion(qty => qty.Value, v => new Quantity(v));
        });
    }
}
