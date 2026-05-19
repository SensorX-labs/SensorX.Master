using MassTransit;
using SensorX.Master.Application.Common.ReadModel;

namespace SensorX.Master.Application.Events.Consumers.ProductSnapshot;

[MessageUrn("Product-Created-Event")]
[EntityName("Product-Created-Event")]
public sealed record CreateProductEvent(
    Guid Id,
    string Code,
    string Name,
    string Manufacture,
    string Unit,
    ProductStatus Status,
    DateTimeOffset CreatedAt
);

[MessageUrn("Product-Updated-Event")]
[EntityName("Product-Updated-Event")]
public sealed record UpdateProductEvent(
    Guid Id,
    string Name,
    string Manufacture,
    string Unit,
    DateTimeOffset? UpdatedAt
);

[MessageUrn("Product-Status-Changed-Event")]
[EntityName("Product-Status-Changed-Event")]
public sealed record ChangeProductStatusEvent(
    Guid Id,
    ProductStatus Status,
    DateTimeOffset? UpdatedAt
);

[MessageUrn("Product-Deleted-Event")]
[EntityName("Product-Deleted-Event")]
public sealed record DeleteProductEvent(Guid Id);

