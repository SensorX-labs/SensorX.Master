using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.Quotes.CreateQuote;

public class CreateQuoteCommand : IRequest<Result<Guid>>
{
    public Guid RFQId { get; set; }
    public Guid CustomerId { get; set; }
    
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;

    public string? Note { get; set; }
    public DateTimeOffset QuoteDate { get; set; } = DateTimeOffset.UtcNow;
    
    public string ShippingAddress { get; set; } = string.Empty;
    public int PaymentTermDays { get; set; }

    public List<CreateQuoteItemCommand> Items { get; set; } = new();
}

public class CreateQuoteItemCommand
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
}
