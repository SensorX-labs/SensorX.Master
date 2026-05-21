using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Domain.Contexts.PaymentContext.AggregateModels;

public class PaymentHistory : IAggregateRoot
{
    public int Id { get; set; }
    public string Gateway { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string? SubAccount { get; set; }
    public string? Code { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string TransferType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TransferAmount { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public int Accumulated { get; set; }
    public PaymentHistoryStatus Status { get; set; }

    public ICollection<PaymentHistoryOrder>? PaymentHistory_Orders { get; set; }
}
