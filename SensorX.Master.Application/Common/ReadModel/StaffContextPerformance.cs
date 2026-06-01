using System;
using MathNet.Numerics.Distributions;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Common.ReadModel
{
    public class StaffContextPerformance : IAggregateRoot
    {
        public Guid StaffId { get; set; }
        public Guid CategoryId { get; set; }

        public int SuccessCount { get; set; } = 0;
        public int FailureCount { get; set; } = 0;
        public double TotalMarginAccumulated { get; set; } = 0;

        // marginOfThisQuote đầu vào tính bằng công thức:
        //  (quote.items.sum(unitPrice - basePrice) / quote.items.sum(basePrice))
        public void RecordSuccess(double marginOfThisQuote)
        {
            SuccessCount += 1; // Chỉ đếm số lượng để nuôi Beta
            TotalMarginAccumulated += marginOfThisQuote; // Lưu dồn tiền lãi
        }

        public void RecordFailure()
        {
            FailureCount += 1;
        }

        // Tính xác suất thành công dựa trên Beta Distribution với smoothing alpha = 1, beta = 1
        public double SampleBetaProbability()
        {
            return Beta.Sample(SuccessCount + 1.0, FailureCount + 1.0);
        }

        // Tính trung bình margin từ dữ liệu cộng dồn
        public double GetAverageMargin()
        {
            if (SuccessCount == 0) return 0;
            
            double avgMargin = TotalMarginAccumulated / SuccessCount;
            
            // Khóa an toàn: Nếu dữ liệu lịch sử tồn dư dạng số nguyên > 1.0, tự động đưa về tỷ lệ thực
            if (avgMargin > 1.0)
            {
                avgMargin /= 100.0;
            }
            
            return avgMargin;
        }

        // Tính giá trị kỳ vọng cuối cùng
        public double CalculateExpectedCategoryScore()
        {
            double probability = SampleBetaProbability();
            double avgMargin = GetAverageMargin();

            // Ví dụ: với probability = 0.7 và avgMargin = 0.3 => 0.7 * (1.0 + 0.3) = 0.91
            return probability * (1.0 + avgMargin);
        }
    }
}