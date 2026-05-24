using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.OrderContext.AggregateModels.OrderAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Queries.Quotes.GetMyQuoteDetail;

public class GetMyQuoteDetailHandler(
    IQueryBuilder<Quote> _quoteQueryBuilder,
    IQueryBuilder<Order> _orderBuilder,
    IQueryBuilder<SaleStaff> _staffBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetMyQuoteDetailQuery, Result<GetMyQuoteDetailResponse>>
{
    public async Task<Result<GetMyQuoteDetailResponse>> Handle(GetMyQuoteDetailQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _quoteQueryBuilder.QueryAsNoTracking.Where(q => q.Id == new QuoteId(request.QuoteId));
            var quote = await _queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
            if (quote is null)
            {
                return Result<GetMyQuoteDetailResponse>.Failure("Không tìm thấy báo giá");
            }
            var customerInfo = new CustomerInfoResponse(
                quote.CustomerId,
                quote.CustomerInfo.CompanyName,
                quote.CustomerInfo.Phone,
                quote.CustomerInfo.Email,
                quote.CustomerInfo.Address,
                quote.CustomerInfo.TaxCode
            );

            var senderInfo = await GetSenderInfo(quote, cancellationToken);
            var (orderId, orderCode) = await GetOrderInfo(quote, cancellationToken);
            var status = GetResponseStatus(quote);

            var quoteItemResponses = quote.LineItems.Select(i => new QuoteItemResponse
            (
                i.Id.Value,
                i.ProductId.Value,
                i.ProductCode.Value,
                i.Manufacturer,
                i.Unit,
                i.Quantity.Value,
                i.UnitPrice.Amount,
                i.TaxRate.Value,
                i.GetLineAmount().Amount,
                i.GetTaxAmount().Amount,
                i.GetTotalLineAmount().Amount
            )).ToList();
            var response = new GetMyQuoteDetailResponse(
                quote.Id,
                quote.Code,
                quote.RFQId,
                orderId,
                orderCode,
                status,
                quote.QuoteDate,
                quote.GetSubtotal().Amount,
                quote.GetTotalTax().Amount,
                quote.GetGrandTotal().Amount,
                quoteItemResponses,
                senderInfo,
                customerInfo
            );


            return Result<GetMyQuoteDetailResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<GetMyQuoteDetailResponse>.Failure($"Lỗi khi lấy chi tiết báo giá: {ex.Message}");
        }
    }

    private async Task<SenderInfoResponse> GetSenderInfo(Quote quote, CancellationToken cancellationToken)
    {
        var query = _staffBuilder.QueryAsNoTracking.Where(s => s.Id == quote.SenderInfo.Id);
        var senderInfo = await _queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
        return new SenderInfoResponse(
            quote.SenderInfo.Id,
            quote.SenderInfo.Name,
            quote.SenderInfo.Email,
            quote.SenderInfo.Phone,
            senderInfo?.AvatarUrl
        );
    }

    private async Task<(Guid? Id, string? Code)> GetOrderInfo(Quote quote, CancellationToken cancellationToken)
    {
        var query = _orderBuilder.QueryAsNoTracking.Where(o => o.QuoteId == quote.Id).Select(o => new { o.Code, o.Id });
        var orderInfo = await _queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
        if (orderInfo == null) return (null, null);
        return (orderInfo.Id, orderInfo.Code);
    }


    private static StatusCustomerCanSeeQuote GetResponseStatus(Quote quote)
    {
        var quoteStatus = quote.Status;
        var quoteResponse = quote.Response;

        if (quoteResponse != null)
        {
            if (quoteResponse.ResponseType == QuoteResponseStatus.Accepted)
                return StatusCustomerCanSeeQuote.Accepted;
            if (quoteResponse.ResponseType == QuoteResponseStatus.Declined)
                return StatusCustomerCanSeeQuote.Declined;
        }

        if (quoteStatus == QuoteStatus.Sent)
            return StatusCustomerCanSeeQuote.Pending;

        return StatusCustomerCanSeeQuote.Expired;
    }
}
