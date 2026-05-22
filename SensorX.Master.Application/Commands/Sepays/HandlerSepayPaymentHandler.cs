using System.Text.RegularExpressions;
using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;
using SensorX.Master.Domain.Contexts.PaymentContext.AggregateModels;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Commands.Sepays
{
    public class HandlerSepayPaymentHandler : IRequestHandler<HandlerPaymentSepayCommand, bool>
    {
        private readonly IRepository<PaymentHistory> _paymentHistoryRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IQueryExecutor _queryExecutor;
        private readonly IUnitOfWork _unitOfWork;

        public HandlerSepayPaymentHandler(
            IRepository<PaymentHistory> paymentHistoryRepository,
            IRepository<Order> orderRepository,
            IRepository<Payment> paymentRepository,
            IQueryExecutor queryExecutor,
            IUnitOfWork unitOfWork)
        {
            _paymentHistoryRepository = paymentHistoryRepository;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _queryExecutor = queryExecutor;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(HandlerPaymentSepayCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orderCodePattern = @"[A-Z]+-\d{6}-\d{9}";
                var content = request.Content ?? request.Description ?? string.Empty;
                var match = Regex.Match(content, orderCodePattern);

                if (match.Success)
                {
                    var orderCode = match.Value;
                    var order = await _queryExecutor.FirstOrDefaultAsync(
                        _orderRepository.AsQueryable()
                            .Where(o => o.Code == orderCode),
                        cancellationToken
                    );

                    if (order is not null)
                    {
                        var payment = await _queryExecutor.FirstOrDefaultAsync(
                            _paymentRepository.AsQueryable()
                                .Where(p => p.OrderId == order.Id),
                            cancellationToken
                        );

                        if (payment is null)
                        {
                            return false;
                        }

                        var existingPaymentHistory = await _queryExecutor.FirstOrDefaultAsync(
                            _paymentHistoryRepository.AsQueryable()
                                .Where(p => p.PaymentId == payment.Id && p.ReferenceCode == (request.ReferenceCode ?? string.Empty)),
                            cancellationToken
                        );

                        if (existingPaymentHistory != null)
                        {
                            return true;
                        }

                        var existingAmounts = await _queryExecutor.ToListAsync(
                            _paymentHistoryRepository.AsQueryable()
                                .Where(p => p.PaymentId == payment.Id)
                                .Select(p => p.TransferAmount),
                            cancellationToken
                        );

                        var paymentHistory = new PaymentHistory(
                            request.Id,
                            request.Gateway ?? string.Empty,
                            request.TransactionDate ?? string.Empty,
                            request.SubAccount,
                            request.Code,
                            request.AccountNumber ?? string.Empty,
                            content,
                            request.TransferType ?? string.Empty,
                            request.Description,
                            request.TransferAmount ?? 0,
                            request.ReferenceCode ?? string.Empty,
                            (int)request.Accumulated!,
                            PaymentHistoryStatus.Finished,
                            payment.Id,
                            order.Id
                        );

                        var totalReceived = existingAmounts.Sum() + paymentHistory.TransferAmount;
                        payment.Reconcile(totalReceived);

                        await _paymentHistoryRepository.AddAsync(paymentHistory, cancellationToken);
                        await _paymentRepository.Update(payment, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
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