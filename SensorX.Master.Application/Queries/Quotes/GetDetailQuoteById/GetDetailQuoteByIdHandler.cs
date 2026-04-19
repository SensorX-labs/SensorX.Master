using MediatR;
using Microsoft.EntityFrameworkCore;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;

namespace SensorX.Master.Application.Queries.Quotes.GetDetailQuoteById;

public class GetDetailQuoteByIdHandler(
    IRepository<Quote> _quoteRepository
) : IRequestHandler<GetDetailQuoteByIdQuery, Result<GetDetailQuoteByIdResponse>>
{
    public async Task<Result<GetDetailQuoteByIdResponse>> Handle(GetDetailQuoteByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Lấy báo giá và nạp các dòng sản phẩm
            var quote = await _quoteRepository.AsQueryable()
                .Include(q => q.LineItems)
                .FirstOrDefaultAsync(q => q.Id == new QuoteId(request.QuoteId), cancellationToken);

            if (quote == null)
            {
                return Result<GetDetailQuoteByIdResponse>.Failure("Không tìm thấy báo giá yêu cầu.");
            }

            var response = new GetDetailQuoteByIdResponse
            {
                Id = quote.Id.Value,
                Code = quote.Code.Value,
                RFQId = quote.RFQId.Value,
                CustomerId = quote.CustomerId.Value,
                Status = quote.Status.ToString(),
                QuoteDate = quote.QuoteDate,
                Note = quote.Note,
                ReasonReject = quote.ReasonReject,

                // Map Customer Info
                RecipientName = quote.CustomerInfo.RecipientName,
                RecipientPhone = quote.CustomerInfo.RecipientPhone.Value,
                CompanyName = quote.CustomerInfo.CompanyName,
                Email = quote.CustomerInfo.Email.Value,
                Address = quote.CustomerInfo.Address,
                TaxCode = quote.CustomerInfo.TaxCode,

                // Map Response Info
                CustomerResponseType = quote.Response?.ResponseType.ToString(),
                ShippingAddress = quote.Response?.ShippingAddress,
                PaymentTerm = quote.Response?.PaymentTerm.ToString(),
                CustomerFeedback = quote.Response?.Feedback,

                // Calculations from Domain
                Subtotal = quote.GetSubtotal().Amount,
                TotalTax = quote.GetTotalTax().Amount,
                GrandTotal = quote.GetGrandTotal().Amount,

                // Map Items
                Items = quote.LineItems.Select(i => new QuoteItemResponse
                {
                    Id = i.Id.Value,
                    ProductId = i.ProductId.Value,
                    ProductCode = i.ProductCode.Value,
                    Manufacturer = i.Manufacturer,
                    Unit = i.Unit,
                    Quantity = i.Quantity.Value,
                    UnitPrice = i.UnitPrice.Amount,
                    TaxRate = i.TaxRate.Value,
                    LineAmount = i.GetLineAmount().Amount,
                    TaxAmount = i.GetTaxAmount().Amount,
                    TotalLineAmount = i.GetTotalLineAmount().Amount
                }).ToList()
            };

            return Result<GetDetailQuoteByIdResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<GetDetailQuoteByIdResponse>.Failure($"Lỗi khi lấy chi tiết báo giá: {ex.Message}");
        }
    }
}
