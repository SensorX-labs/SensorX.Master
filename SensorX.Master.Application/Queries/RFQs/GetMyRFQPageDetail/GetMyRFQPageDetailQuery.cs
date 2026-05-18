using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.RFQs.GetMyRFQPageDetail;

public sealed record GetMyRFQPageDetailQuery(Guid Id) : IRequest<Result<MyRfqDetail>>;

public sealed record MyRfqDetail(
    Guid Id,
    string Code,
    string Status,
    DateTimeOffset CreatedAt,
    MyRfqSaleStaff? SaleStaff,
    MyRfqDetailCustomer? Customer,
    List<MyRfqDetailItem> Items
);

public sealed record MyRfqDetailCustomer(
    Guid Id,
    string Name,
    string Email,
    string? Phone,
    string? Address,
    ShippingInfo? ShippingInfo
);

public sealed record ShippingInfo(
    string RecipientName,
    string RecipientPhone,
    string ShippingAddress
);

public sealed record MyRfqDetailItem(
    Guid ProductId,
    string ProductName,
    string ProductCode,
    double Quantity,
    string Unit
);

public sealed record MyRfqSaleStaff(
    Guid Id,
    string Name,
    string? Phone,
    string Email,
    string? AvatarUrl
);
