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
            if (quote == null)
            {
                return Result<GetDetailQuoteByIdResponse>.Failure("Không tìm thấy báo giá");
            }

            var rfqItems = await _queryExecutor.ToListAsync(
                _rfqQueryBuilder.QueryAsNoTracking
                    .Where(r => r.Id == quote.RFQId)
                    .SelectMany(r => r.Items.Select(ri => new { ri.ProductId, ri.ProductName })),
                cancellationToken
            );
            var rfqItemMap = rfqItems.ToDictionary(x => x.ProductId.Value, x => x.ProductName);

            var rfqCode = await _queryExecutor.FirstOrDefaultAsync(
                _rfqQueryBuilder.QueryAsNoTracking
                    .Where(r => r.Id == quote.RFQId)
                    .Select(r => r.Code.Value),
                cancellationToken
            ) ?? string.Empty;

            var response = new GetDetailQuoteByIdResponse
            (
                quote.Id.Value,
                quote.Code.Value,
                quote.RFQId.Value,
                rfqCode,
                quote.Status,
                quote.QuoteDate,
                quote.Note,
                quote.ReasonReject,

                // Calculations from Domain
                quote.GetSubtotal().Amount,
                quote.GetTotalTax().Amount,
                quote.GetGrandTotal().Amount,

                // Map Items
                quote.LineItems.Select(i => new QuoteItemResponse
                (
                    i.Id.Value,
                    i.ProductId.Value,
                    i.ProductCode.Value,
                    rfqItemMap.TryGetValue(i.ProductId.Value, out var prodName) ? prodName : string.Empty,
                    i.Manufacturer,
                    i.Unit,
                    i.Quantity.Value,
                    i.UnitPrice.Amount,
                    i.TaxRate.Value,
                    i.GetLineAmount().Amount,
                    i.GetTaxAmount().Amount,
                    i.GetTotalLineAmount().Amount
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
                    quote.CustomerId.Value,
                    quote.CustomerInfo.CompanyName,
                    quote.CustomerInfo.Phone.Value,
                    quote.CustomerInfo.Email.Value,
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
