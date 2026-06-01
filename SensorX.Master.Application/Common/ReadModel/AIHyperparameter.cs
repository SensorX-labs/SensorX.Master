using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Common.ReadModel
{
    public class AIHyperparameter : IAggregateRoot
    {
        public int Id { get; set; } // Fixed Id = 1 for the global settings row
        public double K { get; set; } = 1.5;
        public double IdleWeight { get; set; } = 0.1;
        public double LearningRate { get; set; } = 0.01;
    }
}
