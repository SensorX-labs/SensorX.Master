using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Domain.Common;

namespace SensorX.Master.Infrastructure.EntityConfigurations;

public class NotificationEntityConfiguration : IEntityTypeConfiguration<NotificationEntity>
{
    public void Configure(EntityTypeBuilder<NotificationEntity> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id)
            .ValueGeneratedNever();

        builder.Property(n => n.UserId)
            .IsRequired(false);

        builder.Property(n => n.Role)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(n => n.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(n => n.Content)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(n => n.Type)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.TargetUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(n => n.IsRead)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        // Query performance indexes
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.Role);
        builder.HasIndex(n => new { n.UserId, n.IsRead });
        builder.HasIndex(n => new { n.Role, n.IsRead });
    }
}
