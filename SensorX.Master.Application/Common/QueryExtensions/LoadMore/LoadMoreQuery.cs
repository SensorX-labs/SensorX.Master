namespace SensorX.Master.Application.Common.QueryExtensions.LoadMore;

public abstract record LoadMoreQuery
{
    /// <summary>
    /// The number of items to return per page.
    /// </summary>
    public int? PageSize { get; init; }

    /// <summary>
    /// The value of the sorted field for the last item. Used as a cursor for forward navigation.
    /// </summary>
    public string? LastValue { get; init; }

    /// <summary>
    /// The unique identifier of the last item in the current page. Used as a secondary cursor for forward navigation.
    /// </summary>
    public Guid? LastId { get; init; }

    /// <summary>
    /// Set to true for descending order, false for ascending order.
    /// </summary>
    public bool IsDescending { get; init; } = true;
}
