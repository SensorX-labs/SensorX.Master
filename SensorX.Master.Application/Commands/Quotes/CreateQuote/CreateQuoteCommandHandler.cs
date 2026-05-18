using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Common.Exceptions;
using SensorX.Master.Domain.Contexts.QuoteContext;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Commands.Quotes.CreateQuote;

public class CreateQuoteCommandHandler(
    IRepository<Quote> _quoteRepository
) : IRequestHandler<CreateQuoteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateQuoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Use provided QuoteCode from command
            Code quoteCode;
            if (string.IsNullOrWhiteSpace(request.QuoteCode))
            {
                quoteCode = Code.Create("QTE"); // System generates if not provided
            }
            else
            {
                quoteCode = Code.From(request.QuoteCode);
            }

            var quoteId = QuoteId.New();
            var rfqId = new RFQId(request.RFQId);
            var customerId = new CustomerId(request.CustomerId);

            var customerInfo = new CustomerInfo(
                request.RecipientName,
                Phone.From(request.RecipientPhone),
                request.ShippingAddress,
                request.CompanyName,
                Email.From(request.Email),
                request.Address,
                request.TaxCode
            );

            var quote = new Quote(
                quoteId,
                quoteCode,
                rfqId,
                customerId,
                customerInfo,
                request.Note,
                QuoteStatus.Draft,
                null, // No response yet
                request.QuoteDate,
                string.Empty // Initial feedback
            );

            // Add quote items
            if (request.Items != null && request.Items.Count > 0)
            {
                foreach (var item in request.Items)
                {
                    var quoteItem = new QuoteItem(
                        QuoteItemId.New(),
                        new ProductId(item.ProductId),
                        Code.From(item.ProductCode),
                        item.Manufacturer,
                        item.Unit,
                        new Quantity((int)item.Quantity), // Cast to int
                        Money.FromVnd(item.UnitPrice),
                        Percent.From(item.TaxRate)
                    );
                    quote.AddItem(quoteItem);
                }
            }
            else
            {
                return Result<Guid>.Failure("Quote must have at least one item.");
            }

            await _quoteRepository.AddAsync(quote, cancellationToken);
            return Result<Guid>.Success(quote.Id.Value);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}