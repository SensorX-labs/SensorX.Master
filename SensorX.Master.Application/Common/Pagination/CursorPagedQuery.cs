namespace SensorX.Master.Application.Common.Pagination;

public abstract record CursorPagedQuery
{
    public int PageSize { get; init; } = 10;

    // Cursor
    public DateTimeOffset? LastCreatedAt { get; init; }
    public Guid? LastId { get; init; }

    public DateTimeOffset? FirstCreatedAt { get; init; }
    public Guid? FirstId { get; init; }

    public bool IsPrevious { get; init; }
}