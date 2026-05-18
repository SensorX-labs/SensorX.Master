using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.RFQs.GetRFQDetail;

public record GetRFQDetailQuery(Guid Id) : IRequest<Result<GetRFQDetailResponse>>;

public record GetRFQDetailResponse
(
    Guid Id,
    string Code,
    Guid? StaffId,
    string? StaffName,
    Guid CustomerId,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,

    // Flat Customer Info
    string? RecipientName,
    string? RecipientPhone,
    string? ShippingAddress,

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
