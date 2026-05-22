using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Common.Exceptions;
using SensorX.Master.Domain.Contexts.QuoteContext;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Commands.Quotes.UpdateDraftQuote;

public class UpdateDraftQuoteCommandHandler(
    IRepository<Quote> _quoteRepository
) : IRequestHandler<UpdateDraftQuoteCommand, Result>
{
    /// <summary>
    /// Nhân viên sửa báo giá khi ở trạng thái draft hoặc Returned
    /// </summary>
    public async Task<Result> Handle(UpdateDraftQuoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var quote = await _quoteRepository.GetByIdAsync(new QuoteId(request.Id), cancellationToken);
            if (quote is null)
            {
                return Result.Failure("Không tìm thấy báo giá tương ứng");
            }

            // Add quote items
            if (request.Items != null && request.Items.Count > 0)
            {
                UpdateQuoteItems(quote, request.Items);
            }
            else
            {
                return Result.Failure("Quote must have at least one item.");
            }
            quote.SetNote(request.Note);
            await _quoteRepository.SaveChangesAsync(cancellationToken);

            return Result.Success("Cập nhật báo giá thành công");
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }

    private static void UpdateQuoteItems(Quote quote, List<QuoteItemDto> items)
    {
        var mapItems = items.ToDictionary(
            x => x.ProductId,
            x => (UnitPrice: Money.FromVnd(x.UnitPrice), TaxRate: Percent.From(x.TaxRate))
        );
        foreach (var item in quote.LineItems)
        {
            var mapItem = mapItems.GetValueOrDefault(item.ProductId);
            if (mapItem.UnitPrice is not null)
            {
                quote.AddItem(
                    new ProductId(item.ProductId),
                    item.ProductCode,
                    item.Manufacturer ?? "Default",
                    item.Unit,
                    new Quantity((int)item.Quantity),
                    mapItem.UnitPrice,
                    mapItem.TaxRate
                );
            }
            else
            {
                quote.RemoveItem(item.ProductId);
            }
        }
    }
}