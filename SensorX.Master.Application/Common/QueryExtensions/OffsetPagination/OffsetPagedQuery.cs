namespace SensorX.Master.Application.Common.QueryExtensions.OffsetPagination;

public abstract record OffsetPagedQuery(int PageNumber = 1, int PageSize = 10);
