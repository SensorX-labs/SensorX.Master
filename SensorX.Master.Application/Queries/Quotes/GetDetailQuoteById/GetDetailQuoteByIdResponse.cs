namespace SensorX.Master.Application.Queries.Quotes.GetDetailQuoteById;

public class GetDetailQuoteByIdResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid RFQId { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset QuoteDate { get; set; }
    public string? Note { get; set; }
    public string? ReasonReject { get; set; }

    // thong tin khách hàng
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TaxCode { get; set; } = string.Empty;

    // feedback
    public string? CustomerResponseType { get; set; }
    public string? ShippingAddress { get; set; }
    public string? PaymentTerm { get; set; }
    public string? CustomerFeedback { get; set; }

    public decimal Subtotal { get; set; }
    public decimal TotalTax { get; set; }
    public decimal GrandTotal { get; set; }

    public List<QuoteItemResponse> Items { get; set; } = new();
}

public class QuoteItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    
    public decimal LineAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalLineAmount { get; set; }
}
