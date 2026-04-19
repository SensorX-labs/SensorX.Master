using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Queries.RFQs.GetRFQById;

public class GetRFQByIdResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public Guid? StaffId { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Flat Customer Info
    public string RecipientName { get; set; }
    public string RecipientPhone { get; set; }
    public string CompanyName { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public string TaxCode { get; set; }

    public List<RFQItemResponse> Items { get; set; } = new();
}

public class RFQItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductCode { get; set; }
    public int Quantity { get; set; }
    public string Manufacturer { get; set; }
    public string Unit { get; set; }
}
