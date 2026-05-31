using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Infrastructure.EntityConfigurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, v => new PaymentId(v))
            .ValueGeneratedNever();

        builder.Property(p => p.OrderId)
            .HasConversion(id => id.Value, v => new OrderId(v));

        builder.Property(p => p.Amount)
            .HasConversion(m => m.Amount, v => Money.FromVnd(v));

        builder.Property(p => p.PaymentType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.PaymentQRURls)
            .HasColumnType("text[]")
            .IsRequired();
            
        builder.HasOne<Order>()
            .WithMany()
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
