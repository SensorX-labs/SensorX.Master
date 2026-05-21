namespace SensorX.Master.Application.Commands.Quotes.RejectQuote;

using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

public class RejectQuoteHandler(
    IRepository<Quote> _quoteRepository
) : IRequestHandler<RejectQuoteCommand, Result>
{
    /// <summary>
    /// Quản lý từ chối báo giá của nhân viên khi ở trạng thái pending
    /// </summary>
    public async Task<Result> Handle(RejectQuoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var quoteId = new QuoteId(request.Id);
            var quote = await _quoteRepository.GetByIdAsync(quoteId, cancellationToken);

            if (quote is null)
            {
                return Result.Failure("Không tìm thấy báo giá.");
            }

            quote.Reject(request.Reason);
            await _quoteRepository.SaveChangesAsync(cancellationToken);

            return Result.Success("Từ chối báo giá thành công.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Lỗi khi từ chối báo giá: {ex.Message}");
        }
    }
}