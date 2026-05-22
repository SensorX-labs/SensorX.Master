namespace SensorX.Master.Application.Commands.Quotes.WithDraw;

using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

public class WithDrawHandler(
    IRepository<Quote> _quoteRepository
) : IRequestHandler<WithDrawCommand, Result>
{
    /// <summary>
    /// Nhân viên thu hồi báo giá trong quá trình đợi duyệt để sửa
    /// </summary>
    public async Task<Result> Handle(WithDrawCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var quoteId = new QuoteId(request.Id);
            var quote = await _quoteRepository.GetByIdAsync(quoteId, cancellationToken);

            if (quote is null)
            {
                return Result.Failure("Không tìm thấy báo giá.");
            }

            quote.WithDraw();
            await _quoteRepository.SaveChangesAsync(cancellationToken);

            return Result.Success("Thu hồi báo giá thành công.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Lỗi khi thu hồi báo giá: {ex.Message}");
        }
    }
}