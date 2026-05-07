using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Common.Exceptions;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Commands.Quotes.SubmitQuoteForApproval;

public class SubmitQuoteForApprovalHandler(
    IRepository<Quote> _quoteRepository
) : IRequestHandler<SubmitQuoteForApprovalCommand, Result>
{
    public async Task<Result> Handle(SubmitQuoteForApprovalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var quoteId = new QuoteId(request.QuoteId);
            var quote = await _quoteRepository.GetByIdAsync(quoteId, cancellationToken);

            if (quote == null)
            {
                return Result.Failure("Không tìm thấy báo giá.");
            }

            quote.SubmitForApproval();

            await _quoteRepository.UpdateAsync(quote, cancellationToken);

            return Result.Success("Đã gửi báo giá để chờ duyệt thành công.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Lỗi khi gửi báo giá chờ duyệt: {ex.Message}");
        }
    }
}
