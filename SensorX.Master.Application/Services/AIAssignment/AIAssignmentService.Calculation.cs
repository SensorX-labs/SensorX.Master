using System;

namespace SensorX.Master.Application.Services.AIAssignment
{
    public partial class AIAssignmentService
    {
        public static (double UpdatedK, double UpdatedIdleWeight) CalculateGradientUpdate(
            double kOld,
            double idleWeightOld,
            double alpha,
            double finalScore,
            double aggregatedSkillScore,
            double currentWorkload,
            double idleHours,
            bool isSuccess)
        {
            double y = isSuccess ? 1.0 : 0.0;
            double yHat = 1.0 / (1.0 + Math.Exp(-finalScore));
            double error = y - yHat;

            double penaltyWorkload = 1.0 / Math.Pow(currentWorkload + 1.0, kOld);

            // Delta K = error * [ -AggregatedSkillScore * Penalty_workload * ln(CurrentWorkload + 1) ]

            double deltaK = error * (-aggregatedSkillScore * penaltyWorkload * Math.Log(currentWorkload + 1.0));

            // Delta IdleWeight = error * [ tanh(IdleHours / 24) ]
            double deltaIdleWeight = error * Math.Tanh(idleHours / 24.0);

            // Gradient Clipping [-1.0, 1.0]
            double clippedDeltaK = Math.Max(-1.0, Math.Min(1.0, deltaK));
            double clippedDeltaIdleWeight = Math.Max(-1.0, Math.Min(1.0, deltaIdleWeight));

            // Cập nhật và chặn dưới >= 0.0
            double updatedK = Math.Max(0.0, kOld + alpha * clippedDeltaK);
            double updatedIdleWeight = Math.Max(0.0, idleWeightOld + alpha * clippedDeltaIdleWeight);

            return (updatedK, updatedIdleWeight);
        }
    }
}
