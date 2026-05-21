
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using System.Globalization;

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
    public required OrderId OrderId { get; set; }
    
    public PaymentHistory() { }
    
    public PaymentHistory(int id, string gateway, string transactionDate, string? subAccount, string? code, string accountNumber, string content, string transferType, string? description, decimal transferAmount, string referenceCode, int accumulated, PaymentHistoryStatus status, OrderId orderId)
    {
        Id = id;
        Gateway = gateway;
        if (string.IsNullOrWhiteSpace(transactionDate))
            throw new ArgumentException("TransactionDate cannot be null or empty.", nameof(transactionDate));

        // Parse transaction date and force UTC to match PostgreSQL timestamptz expectations.
        TransactionDate = DateTime.Parse(
            transactionDate,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
        SubAccount = subAccount;
        Code = code;
        AccountNumber = accountNumber;
        Content = content;
        TransferType = transferType;
        Description = description;
        TransferAmount = transferAmount;
        ReferenceCode = referenceCode;
        Accumulated = accumulated;
        Status = status;
        OrderId = orderId;
    }
}
