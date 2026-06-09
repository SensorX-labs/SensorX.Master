using SensorX.Master.Domain.Common;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Common.ReadModel
{
    public class SaleStaff : IAggregateRoot
    {
        public SaleStaff(StaffId id, AccountId accountId, Code code, string name, Email email, Phone? phone, StaffStatus status = StaffStatus.Active)
        {
            Id = id;
            AccountId = accountId;
            Code = code;
            Name = name;
            Email = email;
            Phone = phone;
            Status = status;
        }

        public StaffId Id { get; }
        public AccountId AccountId { get; }
        public Code Code { get; }
        public string Name { get; private set; }
        public Email Email { get; private set; }
        public Phone? Phone { get; private set; }
        public string? AvatarUrl { get; set; }
        public StaffStatus Status { get; private set; }

        // Các trường phục vụ hệ thống phân bổ AI
        public int CurrentWorkload { get; set; } = 0;
        public DateTimeOffset? LastAssignedAt { get; set; }

        public void Update(
            string name,
            Email email,
            Phone? phone,
            StaffStatus status
        )
        {
            Name = name;
            Email = email;
            Phone = phone;
            Status = status;
        }

        public void ChangeStatus(StaffStatus status)
        {
            Status = status;
        }

        public void UpdateAvatarUrl(string avatarUrl)
        {
            AvatarUrl = avatarUrl;
        }

        // Hàm thay đổi trạng thái khi nhận đơn
        public void AssignRfq()
        {
            CurrentWorkload++;
            LastAssignedAt = DateTimeOffset.UtcNow;
        }

        // Hàm thay đổi trạng thái khi xong/chê đơn
        public void ReleaseWorkload()
        {
            if (CurrentWorkload > 0)
            {
                CurrentWorkload--;
                if (CurrentWorkload == 0)
                {
                    LastAssignedAt = DateTimeOffset.UtcNow;
                }
            }
        }

        /// <summary>
        /// Hàm tính điểm Phân bổ cuối cùng (Final Score) để chốt người
        /// </summary>
        /// <param name="aggregatedSkillScore">Điểm tổng hợp từ rổ hàng (Tính từ CalculateExpectedCategoryScoren)</param>
        /// <param name="k">Hệ số siết Workload (thường để 1.5 hoặc 2.0)</param>
        /// <param name="idleWeight">Trọng số thưởng thời gian rảnh</param>
        public double CalculateFinalAllocationScore(
            double aggregatedSkillScore,
            double k = 1.5,
            double idleWeight = 0.5)
        {
            // 1. Phạt tải trọng (Workload Penalty)
            // Công thức: 1 / (Workload + 1)^k
            double workloadPenalty = 1.0 / Math.Pow(CurrentWorkload + 1, k);

            // 2. Thưởng rảnh rỗi (Idle Bonus)
            double idleHours = 0;
            if (CurrentWorkload == 0 && LastAssignedAt.HasValue)
            {
                idleHours = (DateTimeOffset.UtcNow - LastAssignedAt.Value).TotalHours;
            }

            double boostIdle = Math.Tanh(idleHours / 24.0) * idleWeight;

            // 3. Ra điểm số chốt hạ
            return (aggregatedSkillScore * workloadPenalty) + boostIdle;
        }
    }
}