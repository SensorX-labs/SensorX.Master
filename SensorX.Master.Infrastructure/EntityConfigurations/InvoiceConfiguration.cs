using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Infrastructure.EntityConfigurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasConversion(id => id.Value, v => new InvoiceId(v))
            .ValueGeneratedNever();

        builder.Property(i => i.Code)
            .HasConversion(c => c.Value, v => Code.From(v));

        builder.Property(i => i.OrderId)
            .HasConversion(id => id.Value, v => new OrderId(v));

        builder.OwnsOne(i => i.BillingInfo, b =>
        {
            b.Property(p => p.CompanyName).HasColumnName("BillingCompanyName");
            b.Property(p => p.TaxCode).HasColumnName("BillingTaxCode");
            b.Property(p => p.Address).HasColumnName("BillingAddress");
            b.Property(p => p.Email).HasConversion(e => e.Value, v => Email.From(v)).HasColumnName("BillingEmail");
        });

        builder.Property(i => i.SubTotal)
            .HasConversion(m => m.Amount, v => Money.FromVnd(v));

        builder.Property(i => i.TaxAmount)
            .HasConversion(m => m.Amount, v => Money.FromVnd(v));

        builder.Property(i => i.GrandTotal)
            .HasConversion(m => m.Amount, v => Money.FromVnd(v));

        builder.Property(i => i.AmountPaid)
            .HasConversion(m => m.Amount, v => Money.FromVnd(v));

        builder.OwnsMany(i => i.Items, item =>
        {
            item.ToTable("InvoiceItems");

            item.HasKey(ii => ii.Id);

            item.Property(ii => ii.Id)
                .HasConversion(id => id.Value, v => new InvoiceItemId(v))
                .ValueGeneratedNever();

            item.Property(ii => ii.ProductId)
                .HasConversion(id => id.Value, v => new ProductId(v));

            item.Property(ii => ii.Quantity)
                .HasConversion(qty => qty.Value, v => new Quantity(v));

            item.Property(ii => ii.UnitPrice)
                .HasConversion(m => m.Amount, v => Money.FromVnd(v));

            item.Property(ii => ii.TaxRate)
                .HasConversion(p => p.Value, v => Percent.From(v));

            item.Property(ii => ii.LineAmount)
                .HasConversion(m => m.Amount, v => Money.FromVnd(v));

            item.Property(ii => ii.TaxAmount)
                .HasConversion(m => m.Amount, v => Money.FromVnd(v));

            item.Property(ii => ii.TotalLineAmount)
                .HasConversion(m => m.Amount, v => Money.FromVnd(v));
        });
    }
}
