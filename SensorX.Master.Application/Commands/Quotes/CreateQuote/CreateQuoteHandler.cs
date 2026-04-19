using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;
using SensorX.Master.Domain.Common.Exceptions;

namespace SensorX.Master.Application.Commands.Quotes.CreateQuote;

public class CreateQuoteHandler(
    IRepository<Quote> _quoteRepository
) : IRequestHandler<CreateQuoteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateQuoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // thông tin customer
            var customerInfo = new CustomerInfo(
                request.RecipientName,
                Phone.From(request.RecipientPhone),
                request.CompanyName,
                Email.From(request.Email),
                request.Address,
                request.TaxCode
            );

            // thông tin báo giá
            var quoteId = QuoteId.New();
            var quoteCode = Code.Create("QTE");
            
            var quote = new Quote(
                quoteId,
                quoteCode,
                new RFQId(request.RFQId),
                new CustomerId(request.CustomerId),
                customerInfo,
                request.Note,
                QuoteStatus.Draft,
                null, // Chưa có phản hồi từ khách hàng
                request.QuoteDate,
                string.Empty
            );

            // thông tin sản phẩm
            if (request.Items == null || !request.Items.Any())
            {
                return Result<Guid>.Failure("Báo giá phải có ít nhất một sản phẩm.");
            }

            // Kiểm tra trùng lặp sản phẩm (Trùng ProductId thì báo lỗi)
            var duplicateProduct = request.Items
                .GroupBy(i => i.ProductId)
                .Where(g => g.Count() > 1)
                .Select(g => g.First().ProductCode)
                .FirstOrDefault();

            if (duplicateProduct != null)
            {
                return Result<Guid>.Failure($"Sản phẩm mã [{duplicateProduct}] bị lặp lại trong danh sách.");
            }

            foreach (var item in request.Items)
            {
                var quoteItem = new QuoteItem(
                    QuoteItemId.New(),
                    new ProductId(item.ProductId),
                    Code.From(item.ProductCode),
                    item.Manufacturer,
                    item.Unit,
                    new Quantity(item.Quantity),
                    Money.FromVnd(item.UnitPrice),
                    Percent.From(item.TaxRate)
                );
                quote.AddItem(quoteItem);
            }

            await _quoteRepository.Add(quote, cancellationToken);
            await _quoteRepository.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(quote.Id.Value);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return Result<Guid>.Failure($"Lỗi hệ thống khi tạo báo giá: {ex.Message}");
        }
    }
}
