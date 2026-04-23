using SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;

namespace SensorX.Master.Application.Queries.RFQs.GetPageListRFQ;

public record GetPageListRFQResponse
(
    Guid Id,
    string Code,
    string Status,
    string RecipientName,
    string RecipientPhone,
    string CompanyName,
    DateTimeOffset CreatedAt,
    Guid? StaffId,
    Guid CustomerId,
    int ItemCount
);

public class RFQOffsetPagedResult : OffsetPagedResult<GetPageListRFQResponse> { }