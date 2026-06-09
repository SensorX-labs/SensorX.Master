using System.Text.RegularExpressions;
using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.InvoiceAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.PaymentAggregate;
using SensorX.Master.Domain.Contexts.PaymentContext.AggregateModels;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Commands.Sepays
{
    public class HandlerSepayPaymentHandler : IRequestHandler<HandlerPaymentSepayCommand, bool>
    {
        private readonly IRepository<PaymentHistory> _paymentHistoryRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly IQueryExecutor _queryExecutor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentNotificationService _paymentNotificationService;

        public HandlerSepayPaymentHandler(
            IRepository<PaymentHistory> paymentHistoryRepository,
            IRepository<Order> orderRepository,
            IRepository<Payment> paymentRepository,
            IRepository<Invoice> invoiceRepository,
            IQueryExecutor queryExecutor,
            IUnitOfWork unitOfWork,
            IPaymentNotificationService paymentNotificationService)
        {
            _paymentHistoryRepository = paymentHistoryRepository;
            _orderRepository = orderRepository;
            _paymentRepository = paymentRepository;
            _invoiceRepository = invoiceRepository;
            _queryExecutor = queryExecutor;
            _unitOfWork = unitOfWork;
            _paymentNotificationService = paymentNotificationService;
        }

        public async Task<bool> Handle(HandlerPaymentSepayCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var orderCodePattern = @"[A-Z]+-\d{6}-\d{9}(?:-[A-Z0-9]+)?";
                var compactPattern = @"([A-Z]+)(\d{6})(\d{9})([A-Z0-9]+)?"; // e.g. ORD260524073547666P1
                Console.WriteLine($"Processing Sepay payment with content: {request.Content}, description: {request.Description}");
                var content = request.Content ?? request.Description ?? string.Empty;

                // try hyphenated first, then compact
                var match = Regex.Match(content, orderCodePattern);
                string? orderCode = null;
                if (match.Success)
                {
                    orderCode = match.Value;
                }
                else
                {
                    var compactMatch = Regex.Match(content, compactPattern);
                    if (compactMatch.Success)
                    {
                        var prefix = compactMatch.Groups[1].Value;
                        var d6 = compactMatch.Groups[2].Value;
                        var d9 = compactMatch.Groups[3].Value;
                        var suffix = compactMatch.Groups[4].Value;
                        orderCode = string.IsNullOrEmpty(suffix) ? $"{prefix}-{d6}-{d9}" : $"{prefix}-{d6}-{d9}-{suffix}";
                    }
                }

                if (orderCode is not null)
                {
                    // normalize by removing hyphens for comparison to handle both hyphenated and compact forms
                    var normalizedCandidate = orderCode.Replace("-", string.Empty);

                    // extract parts to narrow DB query: prefix, yyMMdd (6 digits), ts (9 digits)
                    var partsPattern = @"^([A-Z]+)-?(\d{6})-?(\d{9})(?:-([A-Z0-9]+))?$";
                    var partsMatch = Regex.Match(orderCode, partsPattern);
                    Order? order = null;

                    if (partsMatch.Success)
                    {
                        var prefix = partsMatch.Groups[1].Value;
                        var d6 = partsMatch.Groups[2].Value;
                        var d9 = partsMatch.Groups[3].Value;

                        var candidatesQuery = _orderRepository.AsQueryable()
                            .Where(o => o.Code.Value.StartsWith(prefix) && o.Code.Value.Contains(d6) && o.Code.Value.Contains(d9));

                        var candidates = await _queryExecutor.ToListAsync(candidatesQuery, cancellationToken);
                        order = candidates.FirstOrDefault(o => o.Code.Value == orderCode || o.Code.Value.Replace("-", string.Empty) == normalizedCandidate);
                    }
                    else
                    {
                        // fallback to exact match only (no client-side normalization)
                        order = await _queryExecutor.FirstOrDefaultAsync(
                            _orderRepository.AsQueryable()
                                .Where(o => o.Code.Value == orderCode),
                            cancellationToken
                        );
                    }

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

                        var isFullyPaid = paymentHistory.TransferAmount >= payment.Amount.Amount;

                        if (isFullyPaid)
                        {
                            payment.MarkAsCompleted();
                            order.StartProcessing();
                            await _orderRepository.Update(order, cancellationToken);

                            var invoice = await _queryExecutor.FirstOrDefaultAsync(
                                _invoiceRepository.AsQueryable()
                                    .Where(i => i.OrderId == order.Id),
                                cancellationToken
                            );

                            if (invoice is not null)
                            {
                                var paymentAmount = Money.FromVnd(paymentHistory.TransferAmount);
                                invoice.RecordPayment(paymentAmount);
                                await _invoiceRepository.Update(invoice, cancellationToken);
                            }

                            await _paymentRepository.Update(payment, cancellationToken);
                            await _paymentHistoryRepository.AddAsync(paymentHistory, cancellationToken);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);

                            await _paymentNotificationService.NotifyPaymentStatusChangedAsync(
                                order.Id.Value.ToString(),
                                payment.Status.ToString(),
                                paymentHistory.TransferAmount,
                                cancellationToken);
                        }
                        else
                        {
                            await _paymentHistoryRepository.AddAsync(paymentHistory, cancellationToken);
                            await _unitOfWork.SaveChangesAsync(cancellationToken);
                        }

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