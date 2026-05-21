using System.Text.RegularExpressions;
using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.PaymentContext.AggregateModels;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Commands.Sepays
{
    public class HandlerSepayPaymentHandler : IRequestHandler<HandlerPaymentSepayCommand, bool>
    {
        private readonly IRepository<PaymentHistory> _paymentHistoryRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IQueryExecutor _queryExecutor;

        public HandlerSepayPaymentHandler(
            IRepository<PaymentHistory> paymentHistoryRepository,
            IRepository<Order> orderRepository,
            IQueryExecutor queryExecutor)
        {
            _paymentHistoryRepository = paymentHistoryRepository;
            _orderRepository = orderRepository;
            _queryExecutor = queryExecutor;
        }

        public async Task<bool> Handle(HandlerPaymentSepayCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orderCodePattern = @"[A-Z]+-\d{6}-\d{9}";
                var match = Regex.Match(request.Content!, orderCodePattern);

                if (match.Success)
                {
                    var orderCode = match.Value;
                    // Dùng IQueryExecutor
                    var order = await _queryExecutor.FirstOrDefaultAsync(
                        _orderRepository.AsQueryable()
                            .Where(o => o.Code == orderCode),
                        cancellationToken
                    );

                    if (order is not null)
                    {
                        // Sepay can retry callbacks with the same Id, so skip duplicate inserts.
                        var existingPayment = await _queryExecutor.FirstOrDefaultAsync(
                            _paymentHistoryRepository.AsQueryable()
                                .Where(p => p.Id == request.Id),
                            cancellationToken
                        );

                        if (existingPayment != null)
                        {
                            return true;
                        }

                        var paymentHistory = new PaymentHistory(
                            request.Id!,
                            request.Gateway ?? string.Empty,
                            request.TransactionDate ?? string.Empty,
                            request.SubAccount,
                            request.Code,
                            request.AccountNumber ?? string.Empty,
                            request.Content ?? string.Empty,
                            request.TransferType ?? string.Empty,
                            request.Description,
                            request.TransferAmount ?? 0,
                            request.ReferenceCode ?? string.Empty,
                            (int)request.Accumulated!,
                            PaymentHistoryStatus.Finished,
                            order.Id
                        ){
                            OrderId = order.Id
                        };
                        await _paymentHistoryRepository.AddAsync(paymentHistory, cancellationToken);
                        await _paymentHistoryRepository.SaveChangesAsync(cancellationToken);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing Sepay payment: {ex.Message}", ex);
            }
        }
    }
}