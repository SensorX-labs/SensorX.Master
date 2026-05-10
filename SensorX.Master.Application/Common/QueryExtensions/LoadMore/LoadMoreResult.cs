namespace SensorX.Master.Application.Common.QueryExtensions.LoadMore;

public class LoadMoreResult<T>
{
    public List<T> Items { get; set; } = [];

    public string? LastValue { get; set; }
    public Guid? LastId { get; set; }

    public bool HasNext { get; set; }
}
