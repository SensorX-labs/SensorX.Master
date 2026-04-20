namespace SensorX.Master.Application.Common.Pagination;

public class CursorPagedResult<T>
{
    public List<T> Items { get; set; } = [];

    public DateTimeOffset? FirstCreatedAt { get; set; }
    public Guid? FirstId { get; set; }

    public DateTimeOffset? LastCreatedAt { get; set; }
    public Guid? LastId { get; set; }

    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}