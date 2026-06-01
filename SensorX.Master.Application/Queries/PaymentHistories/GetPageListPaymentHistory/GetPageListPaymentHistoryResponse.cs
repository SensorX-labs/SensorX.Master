namespace SensorX.Master.Application.Queries.PaymentHistories.GetPageListPaymentHistory;

public record GetPageListPaymentHistoryResponse(
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
