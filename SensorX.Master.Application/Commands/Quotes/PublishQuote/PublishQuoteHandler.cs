using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Commands.Quotes.PublishQuote;

public class PublishQuoteHandler(
    IRepository<Quote> _quoteRepository
) : IRequestHandler<PublishQuoteCommand, Result>
{
    /// <summary>
    /// Nhân viên phát hành báo giá cho khách hàng khi ở trạng thái approved
    /// </summary>
    public async Task<Result> Handle(PublishQuoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var quoteId = new QuoteId(request.Id);
            var quote = await _quoteRepository.GetByIdAsync(quoteId, cancellationToken);

            if (quote is null)
            {
                return Result.Failure("Không tìm thấy báo giá.");
            }

            quote.Publish();
            await _quoteRepository.SaveChangesAsync(cancellationToken);

            return Result.Success("Phát hành báo giá thành công.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Lỗi khi phát hành báo giá: {ex.Message}");
        }
    }
}