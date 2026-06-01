using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Application.Common.ReadModel;

namespace SensorX.Master.Infrastructure.EntityConfigurations.ReadModel
{
    public class AIHyperparameterHistoryConfiguration : IEntityTypeConfiguration<AIHyperparameterHistory>
    {
        public void Configure(EntityTypeBuilder<AIHyperparameterHistory> builder)
        {
            builder.ToTable("AIHyperparameterHistories", "read");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Timestamp)
                .IsRequired();


            builder.HasIndex(x => x.Timestamp);
            builder.HasIndex(x => x.RFQId);
            builder.HasIndex(x => x.StaffId);
        }
    }
}
