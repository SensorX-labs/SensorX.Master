using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Queries.Quotes.GetDetailQuoteById;

public class GetDetailQuoteByIdHandler(
    IQueryBuilder<Quote> _quoteQueryBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<GetDetailQuoteByIdQuery, Result<GetDetailQuoteByIdResponse>>
{
    public async Task<Result<GetDetailQuoteByIdResponse>> Handle(GetDetailQuoteByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _quoteQueryBuilder.QueryAsNoTracking
                            .Where(q => q.Id == new QuoteId(request.QuoteId))
                            .Select(q => new GetDetailQuoteByIdResponse
                            (
                                q.Id.Value,
                                q.Code.Value,
                                q.RFQId.Value,
                                q.Status,
                                q.QuoteDate,
                                q.Note,
                                q.ReasonReject,

                                // Calculations from Domain
                                q.GetSubtotal().Amount,
                                q.GetTotalTax().Amount,
                                q.GetGrandTotal().Amount,

                                // Map Items
                                q.LineItems.Select(i => new QuoteItemResponse
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
                                )).ToList(),

                                // Map Sender Info
                                new SenderInfoResponse(
                                    q.SenderInfo.Id,
                                    q.SenderInfo.Name,
                                    q.SenderInfo.Email,
                                    q.SenderInfo.Phone
                                ),

                                // Map Customer Info
                                new CustomerInfoResponse(
                                    q.CustomerId.Value,
                                    q.CustomerInfo.CompanyName,
                                    q.CustomerInfo.Phone.Value,
                                    q.CustomerInfo.Email.Value,
                                    q.CustomerInfo.Address,
                                    q.CustomerInfo.TaxCode
                                ),

                                // Map Customer Feedback (nullable)
                                q.Response != null
                                    ? new QuoteCustomerResponse(
                                        q.Response.ResponseType,
                                        q.Response.PaymentTerm,
                                        q.Response.ShippingAddress,
                                        q.Response.RecipientName,
                                        q.Response.RecipientPhone,
                                        q.Response.Feedback
                                    )
                                    : null
                            ));


            var response = await _queryExecutor.FirstOrDefaultAsync(query, cancellationToken);
            if (response == null)
            {
                return Result<GetDetailQuoteByIdResponse>.Failure("Không tìm thấy báo giá");
            }
            return Result<GetDetailQuoteByIdResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<GetDetailQuoteByIdResponse>.Failure($"Lỗi khi lấy chi tiết báo giá: {ex.Message}");
        }
    }
}
