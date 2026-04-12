using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Infrastructure.EntityConfigurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(id => id.Value, v => new OrderId(v))
            .ValueGeneratedNever();

        builder.Property(o => o.QuoteId)
            .HasConversion(id => id.Value, v => new QuoteId(v));

        builder.Property(o => o.Code)
            .HasConversion(c => c.Value, v => Code.From(v));

        builder.Property(o => o.CustomerId)
            .HasConversion(id => id.Value, v => new CustomerId(v));

        builder.OwnsOne(o => o.CustomerInfo, c =>
        {
            // For Owned entity, column names will by default be prefixed,
            // mapping them explicitly to avoid prefix if preferred, or keeping prefix.
            // As with QuoteConfiguration, we explicitly set column names.
            c.Property(p => p.RecipientName).HasColumnName("CustomerRecipientName");
            c.Property(p => p.RecipientPhone).HasColumnName("CustomerRecipientPhone");
            c.Property(p => p.CompanyName).HasColumnName("CustomerCompanyName");
            c.Property(p => p.Email).HasConversion(e => e.Value, v => Email.From(v)).HasColumnName("CustomerEmail");
            c.Property(p => p.Address).HasColumnName("CustomerAddress");
            c.Property(p => p.TaxCode).HasColumnName("CustomerTaxCode");
        });

        builder.OwnsOne(o => o.SenderInfo, s =>
        {
            s.Property(p => p.Name).HasColumnName("SenderName");
            s.Property(p => p.Email).HasConversion(e => e.Value, v => Email.From(v)).HasColumnName("SenderEmail");
        });

        builder.OwnsMany(o => o.Items, item =>
        {
            item.ToTable("OrderItems");

            item.HasKey(i => i.Id);

            item.Property(i => i.Id)
                .HasConversion(id => id.Value, v => new OrderItemId(v))
                .ValueGeneratedNever();

            item.Property(i => i.ProductId)
                .HasConversion(id => id.Value, v => new ProductId(v));

            item.Property(i => i.ProductCode)
                .HasConversion(c => c.Value, v => Code.From(v));

            item.Property(i => i.Quantity)
                .HasConversion(qty => qty.Value, v => new Quantity(v));

            item.Property(i => i.UnitPrice)
                .HasConversion(m => m.Amount, v => Money.FromVnd(v));

            item.Property(i => i.TaxRate)
                .HasConversion(p => p.Value, v => Percent.From(v));
        });
    }
}
