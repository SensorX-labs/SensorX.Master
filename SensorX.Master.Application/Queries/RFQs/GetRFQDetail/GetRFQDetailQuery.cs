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
    string CompanyName,
    string Phone,
    string Email,
    string Address,
    string TaxCode,
    List<AllocationLogEntryResponse> AllocationLogs,
    List<RejectedLogEntryResponse> RejectedLogs,

    List<RFQItemResponse> Items
);

public record AllocationLogEntryResponse
(
    int Round,
    DateTimeOffset AssignedAt,
    string SnapshotJson
);

public record RejectedLogEntryResponse
(
    Guid StaffId,
    string Reason,
    DateTimeOffset RejectedAt
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
