using System.ComponentModel;
using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.RFQs.GetPageListRFQ;

public class GetPageListRFQQuery : IRequest<Result<PaginatedResult<GetPageListRFQResponse>>>
{
    [DefaultValue(1)]
    public int PageIndex { get; set; } = 1;

    [DefaultValue(10)]
    public int PageSize { get; set; } = 10;

    public string? SearchTerm { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? StaffId { get; set; }
}
