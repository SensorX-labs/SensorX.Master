namespace SensorX.Master.Application.Common.QueryExtensions.KeysetPagination;

public abstract record KeysetPagedQuery
{
    /// <summary>
    /// The number of items to return per page. Max value is typically limited by the server (e.g., 100).
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// The creation timestamp of the last item in the current page. Used as a cursor for forward navigation (Next Page).
    /// </summary>
    public DateTimeOffset? LastCreatedAt { get; init; }

    /// <summary>
    /// The unique identifier of the last item in the current page. Used as a secondary cursor for forward navigation.
    /// </summary>
    public Guid? LastId { get; init; }

    /// <summary>
    /// The creation timestamp of the first item in the current page. Used as a cursor for backward navigation (Previous Page).
    /// </summary>
    public DateTimeOffset? FirstCreatedAt { get; init; }

    /// <summary>
    /// The unique identifier of the first item in the current page. Used as a secondary cursor for backward navigation.
    /// </summary>
    public Guid? FirstId { get; init; }

    /// <summary>
    /// Set to true to navigate to the previous page relative to the provided cursors.
    /// </summary>
    public bool IsPrevious { get; init; }
}