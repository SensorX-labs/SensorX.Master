using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Queries.Quotes.GetDetailQuoteById;

public class GetDetailQuoteByIdHandler(
    IQueryBuilder<Quote> _quoteQueryBuilder,
    IQueryBuilder<RFQ> _rfqQueryBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetDetailQuoteByIdQuery, Result<GetDetailQuoteByIdResponse>>
{
    public async Task<Result<GetDetailQuoteByIdResponse>> Handle(GetDetailQuoteByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _quoteQueryBuilder.QueryAsNoTracking
                            .Where(q => q.Id == new QuoteId(request.QuoteId));

            var quote = await _queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
            if (quote is null)
            {
                return Result<GetDetailQuoteByIdResponse>.Failure("Không tìm thấy báo giá");
            }

            var rfqItems = await _queryExecutor.ToListAsync(
                _rfqQueryBuilder.QueryAsNoTracking
                    .Where(r => r.Id == quote.RFQId)
                    .SelectMany(r => r.Items.Select(ri => new { ri.ProductId, ri.ProductName })),
                cancellationToken
            );
            var rfqItemMap = rfqItems.ToDictionary(x => x.ProductId, x => x.ProductName);

            var response = new GetDetailQuoteByIdResponse
            (
                quote.Id,
                quote.Code,
                quote.RFQId,
                quote.Status,
                quote.QuoteDate,
                quote.Note,
                quote.ReasonReject,

                // Calculations from Domain
                quote.GetSubtotal(),
                quote.GetTotalTax(),
                quote.GetGrandTotal(),

                // Map Items
                quote.LineItems.Select(i => new QuoteItemResponse
                (
                    i.Id,
                    i.ProductId,
                    i.ProductCode,
                    i.ProductName,
                    i.Manufacturer,
                    i.Unit,
                    i.Quantity,
                    i.UnitPrice,
                    i.TaxRate,
                    i.GetLineAmount(),
                    i.GetTaxAmount(),
                    i.GetTotalLineAmount()
                )).ToList(),

                // Map Sender Info
                new SenderInfoResponse(
                    quote.SenderInfo.Id,
                    quote.SenderInfo.Name,
                    quote.SenderInfo.Email,
                    quote.SenderInfo.Phone
                ),

                // Map Customer Info
                new CustomerInfoResponse(
                    quote.CustomerId,
                    quote.CustomerInfo.CompanyName,
                    quote.CustomerInfo.Phone,
                    quote.CustomerInfo.Email,
                    quote.CustomerInfo.Address,
                    quote.CustomerInfo.TaxCode
                ),

                // Map Customer Feedback (nullable)
                quote.Response != null
                    ? new QuoteCustomerResponse(
                        quote.Response.ResponseType,
                        quote.Response.PaymentTerm,
                        quote.Response.ShippingAddress,
                        quote.Response.RecipientName,
                        quote.Response.RecipientPhone,
                        quote.Response.Feedback
                    )
                    : null
            );

            return Result<GetDetailQuoteByIdResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<GetDetailQuoteByIdResponse>.Failure($"Lỗi khi lấy chi tiết báo giá: {ex.Message}");
        }
    }
}
