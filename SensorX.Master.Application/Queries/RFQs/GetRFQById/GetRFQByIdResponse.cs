using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;

namespace SensorX.Master.Application.Queries.RFQs.GetRFQById;

public record GetRFQByIdResponse
(
    Guid Id,
    string Code,
    Guid? StaffId,
    Guid CustomerId,
    string Status,
    DateTimeOffset CreatedAt,

    // Flat Customer Info
    string RecipientName,
    string RecipientPhone,
    string CompanyName,
    string Email,
    string Address,
    string TaxCode,

    List<RFQItemResponse> Items
);

public record RFQItemResponse
(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductCode,
    int Quantity,
    string? Manufacturer,
    string Unit
);
