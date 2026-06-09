using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.PaymentHistories.GetDetailPaymentHistory;

public record GetDetailPaymentHistoryQuery(int Id) : IRequest<Result<GetDetailPaymentHistoryResponse>>;
