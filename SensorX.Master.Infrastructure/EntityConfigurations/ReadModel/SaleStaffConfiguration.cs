using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Application.Common.ReadModel;

namespace SensorX.Master.Infrastructure.EntityConfigurations.ReadModel;

public class SaleStaffConfiguration : IEntityTypeConfiguration<SaleStaff>
{
    public void Configure(EntityTypeBuilder<SaleStaff> builder)
    {
        builder.ToTable("SaleStaffSnapshots", "read");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, v => new Domain.StrongIDs.StaffId(v))
            .ValueGeneratedNever();

        builder.Property(x => x.AccountId)
            .HasConversion(id => id.Value, v => new Domain.StrongIDs.AccountId(v));

        builder.Property(x => x.Code)
            .HasConversion(c => c.Value, v => Domain.ValueObjects.Code.From(v));

        builder.Property(x => x.Email)
            .HasConversion(e => e.Value, v => Domain.ValueObjects.Email.From(v));

        builder.Property(x => x.Phone)
            .HasConversion(p => p != null ? p.Value : null, v => v != null ? Domain.ValueObjects.Phone.From(v) : null);
    }
}