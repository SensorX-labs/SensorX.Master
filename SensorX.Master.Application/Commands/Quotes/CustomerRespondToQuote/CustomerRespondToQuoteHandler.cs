using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Common.Exceptions;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Commands.Quotes.CustomerRespondToQuote;

public class CustomerRespondToQuoteHandler(
    IRepository<Quote> _quoteRepository
) : IRequestHandler<CustomerRespondToQuoteCommand, Result>
{
    public async Task<Result> Handle(CustomerRespondToQuoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var quoteId = new QuoteId(request.Id);
            var quote = await _quoteRepository.GetByIdAsync(quoteId, cancellationToken);

            if (quote is null)
            {
                return Result.Failure("Không tìm thấy thông tin báo giá.");
            }
            if (quote.Response != null)
            {
                return Result.Warning("Báo giá đã được phản hồi. Vui lòng chờ phản hồi từ nhân viên kinh doanh.");
            }

            var response = new QuoteResponse(
                request.ResponseType,
                request.PaymentTerm,
                request.ShippingAddress ?? quote.CustomerInfo.Address,
                request.RecipientName,
                request.RecipientPhone,
                request.Feedback
            );
            quote.ChangeStatus(QuoteStatus.Sent);
            quote.RecordCustomerResponse(response);
            await _quoteRepository.UpdateAsync(quote, cancellationToken);

            return Result.Success("Phản hồi báo giá thành công.");
        }
        catch (Exception ex)
        {
            return Result.Failure($"Lỗi khi phản hồi báo giá: {ex.Message}");
        }
    }
}
