using MediatR;
using Microsoft.Extensions.Logging;
using SensorX.Master.Application.Common.DomainEvent;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Application.Services.AIAssignment.Models;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Events.DomainEvents.Quote.CustomerRespondedQuote;

public class UpdateAIAfterQuoteRespondedEventHandler(
    IRepository<StaffContextPerformance> _performanceRepository,
    IQueryBuilder<StaffContextPerformance> _performanceBuilder,
    IQueryExecutor _queryExecutor,
    IRepository<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate.Quote> _quoteRepository,
    IRepository<SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate.RFQ> _rfqRepository,
    IRepository<AIHyperparameter> _hyperparameterRepository,
    IQueryBuilder<AIHyperparameter> _hyperparameterBuilder,
    ILogger<UpdateAIAfterQuoteRespondedEventHandler> _logger
) : INotificationHandler<DomainEventNotification<CustomerRespondedQuoteEvent>>
{
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
        await _performanceRepository.SaveChangesAsync(cancellationToken);

        // Cập nhật tham số AI trực tuyến (Online Gradient Update)
        var rfq = await _rfqRepository.GetByIdAsync(quote.RFQId, cancellationToken);
        if (rfq != null && rfq.AllocationLogs.Any())
        {
            var latestLog = rfq.AllocationLogs.OrderByDescending(l => l.Round).FirstOrDefault();
            if (latestLog != null)
            {
                try
                {
                    var snapshots = System.Text.Json.JsonSerializer.Deserialize<List<AllocationSnapshot>>(
                        latestLog.SnapshotJson,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    var staffSnapshot = snapshots?.FirstOrDefault(s => s.StaffId == staffId.Value);
                    if (staffSnapshot != null)
                    {
                        var hyperparamQuery = _hyperparameterBuilder.QueryAsNoTracking.Where(h => h.Id == 1);
                        var hyperparams = await _queryExecutor.FirstOrDefaultAsync(hyperparamQuery, cancellationToken);
                        if (hyperparams != null)
                        {
                            double finalScore = staffSnapshot.FinalScore;
                            double aggregatedSkillScore = staffSnapshot.AggregatedSkillScore;
                            double currentWorkload = staffSnapshot.CurrentWorkload;
                            double idleHours = staffSnapshot.IdleHours;

                            double kOld = hyperparams.K;
                            double idleWeightOld = hyperparams.IdleWeight;
                            double alpha = hyperparams.LearningRate;

                            double y = isSuccess ? 1.0 : 0.0;
                            double yHat = 1.0 / (1.0 + Math.Exp(-finalScore));
                            double error = y - yHat;

                            double penaltyWorkload = 1.0 / Math.Pow(currentWorkload + 1.0, kOld);
                            
                            // Delta K = error * [ -AggregatedSkillScore * Penalty_workload * ln(CurrentWorkload + 1) ]
                            double deltaK = error * (-aggregatedSkillScore * penaltyWorkload * Math.Log(currentWorkload + 1.0));

                            // Delta IdleWeight = error * [ (1 - tanh^2(IdleHours / 24)) * (1 / 24) ]
                            double tanhVal = Math.Tanh(idleHours / 24.0);
                            double deltaIdleWeight = error * ((1.0 - (tanhVal * tanhVal)) / 24.0);

                            // Gradient Clipping [-1.0, 1.0]
                            double clippedDeltaK = Math.Max(-1.0, Math.Min(1.0, deltaK));
                            double clippedDeltaIdleWeight = Math.Max(-1.0, Math.Min(1.0, deltaIdleWeight));

                            // Cập nhật và chặn dưới >= 0.0
                            hyperparams.K = Math.Max(0.0, kOld + alpha * clippedDeltaK);
                            hyperparams.IdleWeight = Math.Max(0.0, idleWeightOld + alpha * clippedDeltaIdleWeight);

                            _logger.LogInformation("Online Gradient Update: K updated {KOld} -> {KNew}, IdleWeight updated {IdleWeightOld} -> {IdleWeightNew}", 
                                kOld, hyperparams.K, idleWeightOld, hyperparams.IdleWeight);

                            await _hyperparameterRepository.Update(hyperparams, cancellationToken);
                            await _hyperparameterRepository.SaveChangesAsync(cancellationToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật siêu tham số AI trực tuyến cho RFQ {RfqId}", quote.RFQId.Value);
                }
            }
        }
    }
}
