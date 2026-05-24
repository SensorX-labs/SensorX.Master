using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Application.Common.ReadModel;

namespace SensorX.Master.Infrastructure.EntityConfigurations.ReadModel;

public class StaffContextPerformanceConfiguration : IEntityTypeConfiguration<StaffContextPerformance>
{
    public void Configure(EntityTypeBuilder<StaffContextPerformance> builder)
    {
        builder.ToTable("StaffContextPerformances", "read");
        
        builder.HasKey(s => new { s.StaffId, s.CategoryId });
        
        builder.Property(s => s.StaffId).IsRequired();
        builder.Property(s => s.CategoryId).IsRequired();
        
        builder.Property(s => s.SuccessCount).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.FailureCount).IsRequired().HasDefaultValue(0);
        builder.Property(s => s.TotalMarginAccumulated).IsRequired().HasDefaultValue(0);
    }
}
