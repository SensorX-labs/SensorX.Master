namespace SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;

public abstract record OffsetPagedQuery
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
