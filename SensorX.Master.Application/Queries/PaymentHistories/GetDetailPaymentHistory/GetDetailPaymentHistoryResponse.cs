namespace SensorX.Master.Application.Queries.PaymentHistories.GetDetailPaymentHistory;

public record GetDetailPaymentHistoryResponse(
    int Id,
    string Gateway,
    DateTime TransactionDate,
    string? SubAccount,
    string? Code,
    string AccountNumber,
    string Content,
    string TransferType,
    string? Description,
    decimal TransferAmount,
    string ReferenceCode,
    int Accumulated,
    string Status,
    Guid PaymentId,
    Guid OrderId
);
