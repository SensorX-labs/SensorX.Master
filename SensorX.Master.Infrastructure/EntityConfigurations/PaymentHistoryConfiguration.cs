using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;
using SensorX.Master.Domain.Contexts.PaymentContext.AggregateModels;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Infrastructure.EntityConfigurations;

public class PaymentHistoryConfiguration : IEntityTypeConfiguration<PaymentHistory>
{
    public void Configure(EntityTypeBuilder<PaymentHistory> builder)
    {
      builder.ToTable("Payment_History");
      builder.HasKey(ph => ph.Id);

      builder.Property(x => x.Id).ValueGeneratedNever();
      builder.Property(ph => ph.Gateway).IsRequired();
      builder.Property(ph => ph.TransactionDate).IsRequired();
      builder.Property(ph => ph.AccountNumber).IsRequired();
      builder.Property(ph => ph.Content).IsRequired();
      builder.Property(ph => ph.TransferType).IsRequired();
      builder.Property(ph => ph.TransferAmount).IsRequired().HasColumnType("decimal(18,0)");
      builder.Property(ph => ph.ReferenceCode).IsRequired();
      builder.Property(ph => ph.Accumulated).IsRequired();
      builder.Property(ph => ph.Status).IsRequired();
      builder.Property(ph => ph.PaymentId).IsRequired().HasConversion(x => x.Value, x => new PaymentId(x));
      builder.Property(ph => ph.OrderId).IsRequired().HasConversion(x => x.Value, x => new OrderId(x));

      builder.HasOne<Payment>()
        .WithMany()
        .HasForeignKey(ph => ph.PaymentId)
        .OnDelete(DeleteBehavior.Cascade);

      builder.HasIndex(ph => new { ph.PaymentId, ph.ReferenceCode }).IsUnique();
      builder.HasIndex(ph => ph.OrderId);
    }
}
