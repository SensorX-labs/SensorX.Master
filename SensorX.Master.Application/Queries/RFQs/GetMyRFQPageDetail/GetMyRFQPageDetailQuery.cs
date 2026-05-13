using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.RFQs.GetMyRFQPageDetail;

public sealed record GetMyRFQPageDetailQuery(Guid Id) : IRequest<Result<MyRfqDetail>>;

public sealed record MyRfqDetail(
    Guid Id,
    string Code,
    string Status,
    DateTimeOffset CreatedAt,
    Guid CustomerId,
    string? RecipientName,
    string? RecipientPhone,
    string? Email,
    string? Address,
    string? CompanyName,
    MyRfqDetailCustomer? Customer,
    List<MyRfqDetailItem> Items
);

public sealed record MyRfqDetailCustomer(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? Address
);

public sealed record MyRfqDetailItem(
    Guid ProductId,
    string ProductName,
    string ProductCode,
    double Quantity,
    string Unit
);