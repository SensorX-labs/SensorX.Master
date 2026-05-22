using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Common.Exceptions;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Commands.Quotes.ApproveQuote;

public class ApproveQuoteHandler(
    IRepository<Quote> _quoteRepository
) : IRequestHandler<ApproveQuoteCommand, Result>
{
    /// <summary>
    /// Quản lý phê duyệt báo giá của nhân viên khi ở trạng thái pending
    /// </summary>
    public async Task<Result> Handle(ApproveQuoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var quoteId = new QuoteId(request.QuoteId);
            var quote = await _quoteRepository.GetByIdAsync(quoteId, cancellationToken);

            if (quote is null)
            {
                return Result.Failure("Không tìm thấy báo giá.");
            }

            quote.Approve();

            await _quoteRepository.SaveChangesAsync(cancellationToken);

            return Result.Success("Phê duyệt báo giá thành công.");
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Lỗi khi phê duyệt báo giá: {ex.Message}");
        }
    }
}
