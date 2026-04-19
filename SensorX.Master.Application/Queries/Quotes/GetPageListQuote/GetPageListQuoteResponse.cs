namespace SensorX.Master.Application.Queries.Quotes.GetPageListQuote;

public class GetPageListQuoteResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset QuoteDate { get; set; }
    public Guid CustomerId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public int ItemCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
