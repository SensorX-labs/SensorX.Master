using System.ComponentModel;

namespace SensorX.Master.Domain.Contexts.PaymentContext.AggregateModels;

public enum PaymentHistoryStatus
{
    /// <summary> Chưa thực hiện </summary>
    [Description("Chưa thực hiện")]
    Pendding = 0,
    /// <summary> Đã thực hiện </summary>
    [Description("Đã thực hiện")]
    Finished = 1,
}

