using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Services.AIAssignment.Models
{
    public class AllocationSnapshot
    {
        public Guid StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public double AggregatedSkillScore { get; set; }
        public double CurrentWorkload { get; set; }
        public double IdleHours { get; set; }
        public double FinalScore { get; set; }
        public double K { get; set; }
        public double IdleWeight { get; set; }
    }
}
