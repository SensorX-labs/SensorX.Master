using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Services.AIAssignment;
using SensorX.Master.Application.Services.AIAssignment.Models;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Events.DomainEvents.Quote.CustomerRespondedQuote;

public class UpdateAIAfterQuoteRespondedEventHandler(
    IRepository<StaffContextPerformance> _performanceRepository,
    IQueryBuilder<StaffContextPerformance> _performanceBuilder,
    IQueryExecutor _queryExecutor,
    IRepository<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.Quote> _quoteRepository,
    IRepository<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate.RFQ> _rfqRepository,
    IRepository<AIHyperparameter> _hyperparameterRepository,
    IRepository<AIHyperparameterHistory> _hyperparameterHistoryRepository,
    IUnitOfWork _unitOfWork,
    ILogger<UpdateAIAfterQuoteRespondedEventHandler> _logger
) : INotificationHandler<DomainEventNotification<CustomerRespondedQuoteEvent>>
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task Handle(DomainEventNotification<CustomerRespondedQuoteEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        bool isSuccess = domainEvent.QuoteResponse.ResponseType == QuoteResponseStatus.Accepted;
        await ProcessQuoteResponse(domainEvent.QuoteId, isSuccess, cancellationToken);
    }

    private async Task ProcessQuoteResponse(QuoteId quoteId, bool isSuccess, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Đang cập nhật ký ức AI sau khi Quote {Id} phản hồi ({Status}).", quoteId.Value, isSuccess ? "Accepted" : "Rejected");

        var quote = await _quoteRepository.GetByIdAsync(quoteId, cancellationToken);
        if (quote is null) return;

        var staffId = quote.SenderInfo.Id; // Lấy ID của SaleStaff tạo Quote
        var categoryIds = quote.LineItems.Select(x => x.CategoryId.Value).Distinct().ToList();

        var query = _performanceBuilder.QueryAsNoTracking.Where(p => p.StaffId == staffId && categoryIds.Contains(p.CategoryId));
        var existingPerformances = await _queryExecutor.ToListAsync(query, cancellationToken);

        foreach (var categoryId in categoryIds)
        {
            var perf = existingPerformances.FirstOrDefault(p => p.CategoryId == categoryId);
            bool isNew = false;
            if (perf == null)
            {
                perf = new StaffContextPerformance
                {
                    StaffId = staffId,
                    CategoryId = categoryId,
                    SuccessCount = 0,
                    FailureCount = 0,
                    TotalMarginAccumulated = 0
                };
                isNew = true;
            }

            if (isSuccess)
            {
                double totalFloorAmount = 0;
                double totalProfit = 0;
                foreach (var item in quote.LineItems.Where(i => i.CategoryId.Value == categoryId))
                {
                    totalProfit += (double)((item.UnitPrice.Amount - item.FloorPrice.Amount) * item.Quantity.Value);
                    totalFloorAmount += (double)(item.FloorPrice.Amount * item.Quantity.Value);
                }
                double margin = totalFloorAmount > 0 ? totalProfit / totalFloorAmount : 0;

                // ==========================================
                // BỘ LỌC CHUẨN HÓA ĐÁNH CHẶN LỖI SỐ NGUYÊN / OUTLIERS
                // ==========================================
                // Nếu margin vọt lên dạng số nguyên (ví dụ: lãi 31.7% nhưng hệ thống trả về số thực 31.7)
                if (margin > 1.0 && margin <= 100.0)
                {
                    margin /= 100.0; // Ép về tỷ lệ thực chuẩn: 0.317
                }
                // Ngăn chặn Outlier cực đoan hoặc lỗi chia cho số âm/số quá nhỏ trong DB
                else if (margin > 100.0 || margin < 0)
                {
                    margin = 0.3; // Fallback an toàn về mức biên lợi nhuận trung bình ngành (30%)
                }
                // ==========================================

                perf.RecordSuccess(margin);
            }
            else
            {
                perf.RecordFailure();
            }

            if (isNew)
            {
                await _performanceRepository.Add(perf, cancellationToken);
            }
            else
            {
                await _performanceRepository.Update(perf, cancellationToken);
            }
        }

        // Cập nhật tham số AI trực tuyến (Online Gradient Update)
        var rfq = await _rfqRepository.GetByIdAsync(quote.RFQId, cancellationToken);
        if (rfq is not null && rfq.AllocationLogs.Count != 0)
        {
            var latestLog = rfq.AllocationLogs.OrderByDescending(l => l.Round).FirstOrDefault();
            if (latestLog != null)
            {
                try
                {
                    var snapshots = JsonSerializer.Deserialize<List<AllocationSnapshot>>(
                        latestLog.SnapshotJson,
                        _jsonOptions
                    );

                    var staffSnapshot = snapshots?.FirstOrDefault(s => s.StaffId == staffId.Value);
                    if (staffSnapshot != null)
                    {
                        var hyperparams = await _hyperparameterRepository.GetByIdAsync(1, cancellationToken);
                        bool isNewHyperparams = false;
                        if (hyperparams == null)
                        {
                            hyperparams = new AIHyperparameter { Id = 1, K = 1.5, IdleWeight = 0.1, LearningRate = 0.01 };
                            isNewHyperparams = true;
                        }

                        double finalScore = staffSnapshot.FinalScore;
                        double aggregatedSkillScore = staffSnapshot.AggregatedSkillScore;
                        double currentWorkload = staffSnapshot.CurrentWorkload;
                        double idleHours = staffSnapshot.IdleHours;

                        double kOld = hyperparams.K;
                        double idleWeightOld = hyperparams.IdleWeight;
                        double alpha = hyperparams.LearningRate;

                        var (updatedK, updatedIdleWeight) = AIAssignmentService.CalculateGradientUpdate(
                            kOld,
                            idleWeightOld,
                            alpha,
                            finalScore,
                            aggregatedSkillScore,
                            currentWorkload,
                            idleHours,
                            isSuccess
                        );

                        hyperparams.K = updatedK;
                        hyperparams.IdleWeight = updatedIdleWeight;

                        _logger.LogInformation("Online Gradient Update: K updated {KOld} -> {KNew}, IdleWeight updated {IdleWeightOld} -> {IdleWeightNew}",
                            kOld, hyperparams.K, idleWeightOld, hyperparams.IdleWeight);

                        if (isNewHyperparams)
                        {
                            await _hyperparameterRepository.Add(hyperparams, cancellationToken);
                        }
                        else
                        {
                            await _hyperparameterRepository.Update(hyperparams, cancellationToken);
                        }


                        double yHat = 1.0 / (1.0 + Math.Exp(-finalScore));

                        // Lưu lịch sử biến thiên
                        var history = new AIHyperparameterHistory
                        {
                            RFQId = quote.RFQId.Value,
                            StaffId = staffId.Value,
                            IsSuccess = isSuccess,
                            PredictedScore = yHat, // Lưu xác suất dự báo thay vì điểm thô
                            KBefore = kOld,
                            KAfter = updatedK,
                            DeltaK = updatedK - kOld,
                            IdleWeightBefore = idleWeightOld,
                            IdleWeightAfter = updatedIdleWeight,
                            DeltaIdleWeight = updatedIdleWeight - idleWeightOld,
                            Loss = isSuccess ? -Math.Log(yHat + 1e-9) : -Math.Log(1.0 - yHat + 1e-9)
                        };

                        await _hyperparameterHistoryRepository.Add(history, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật siêu tham số AI trực tuyến cho RFQ {RfqId}", quote.RFQId.Value);
                }
            }
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
