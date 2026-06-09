using System;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Common.ReadModel
{
    public class AIHyperparameterHistory : IAggregateRoot
    {
        public Guid Id { get; set; } = Guid.CreateVersion7();
        public Guid RFQId { get; set; }
        public Guid StaffId { get; set; }


        public bool IsSuccess { get; set; }
        public double PredictedScore { get; set; }


        public double KBefore { get; set; }
        public double KAfter { get; set; }
        public double DeltaK { get; set; }


        public double IdleWeightBefore { get; set; }
        public double IdleWeightAfter { get; set; }
        public double DeltaIdleWeight { get; set; }


        public double Loss { get; set; }


        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}
