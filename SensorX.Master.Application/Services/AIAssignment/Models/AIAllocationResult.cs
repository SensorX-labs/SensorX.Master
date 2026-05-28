using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Services.AIAssignment.Models
{
    public class AIAllocationResult
    {
        public StaffId? WinnerStaffId { get; set; }
        public List<AllocationSnapshot> CandidatesSnapshot { get; set; } = new();
    }
}
