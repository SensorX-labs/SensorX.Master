using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Infrastructure.EntityConfigurations;

public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quotes");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Id)
            .HasConversion(id => id.Value, v => new QuoteId(v))
            .ValueGeneratedNever();

        builder.Property(q => q.Code)
            .HasConversion(c => c.Value, v => Code.From(v));

        builder.Property(q => q.RFQId)
            .HasConversion(id => id.Value, v => new RFQId(v));

        builder.Property(q => q.CustomerId)
            .HasConversion(id => id.Value, v => new CustomerId(v));

        builder.OwnsOne(q => q.CustomerInfo, c =>
        {
            c.Property(p => p.RecipientName).HasColumnName("RecipientName");
            c.Property(p => p.RecipientPhone).HasColumnName("RecipientPhone");
            c.Property(p => p.CompanyName).HasColumnName("CompanyName");
            c.Property(p => p.Email).HasColumnName("Email");
            c.Property(p => p.Address).HasColumnName("Address");
            c.Property(p => p.TaxCode).HasColumnName("TaxCode");
        });

        builder.OwnsOne(q => q.Response, r =>
        {
            r.Property(p => p.ResponseType).HasColumnName("ResponseType");
            r.Property(p => p.PaymentTerm).HasColumnName("PaymentTerm");
            r.Property(p => p.ShippingAddress).HasColumnName("ShippingAddress");
            r.Property(p => p.Feedback).HasColumnName("Feedback");
        });

        builder.OwnsMany(q => q.LineItems, item =>
        {
            item.ToTable("QuoteItems");

            item.HasKey(i => i.Id);

            item.Property(i => i.Id)
                .HasConversion(id => id.Value, v => new QuoteItemId(v))
                .ValueGeneratedNever();

            item.Property(i => i.ProductCode)
                .HasConversion(c => c.Value, v => Code.From(v));

            item.Property(i => i.ProductId)
                .HasConversion(id => id.Value, v => new ProductId(v));

            item.Property(i => i.Quantity)
                .HasConversion(qty => qty.Value, v => new Quantity(v));

            item.Property(i => i.UnitPrice)
                .HasConversion(m => m.Amount, v => Money.FromVnd(v));

            item.Property(i => i.TaxRate)
                .HasConversion(p => p.Value, v => Percent.From(v));
        });
    }
}