using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SensorX.Master.Application.Common.ReadModel;

namespace SensorX.Master.Infrastructure.EntityConfigurations.ReadModel
{
    public class AIHyperparameterConfiguration : IEntityTypeConfiguration<AIHyperparameter>
    {
        public void Configure(EntityTypeBuilder<AIHyperparameter> builder)
        {
            builder.ToTable("AIHyperparameters", "read");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.Id)
                .ValueGeneratedNever();

            builder.Property(h => h.K)
                .IsRequired()
                .HasDefaultValue(3.0);

            builder.Property(h => h.IdleWeight)
                .IsRequired()
                .HasDefaultValue(1.0);

            builder.Property(h => h.LearningRate)
                .IsRequired()
                .HasDefaultValue(0.01);

            builder.HasData(new AIHyperparameter
            {
                Id = 1,
                K = 3.0,
                IdleWeight = 1.0,
                LearningRate = 0.01
            });
        }
    }
}
