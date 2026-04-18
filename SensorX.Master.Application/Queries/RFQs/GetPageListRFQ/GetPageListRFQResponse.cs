namespace SensorX.Master.Application.Queries.RFQs.GetPageListRFQ;

public class GetPageListRFQResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Status { get; set; }
    public string RecipientName { get; set; }
    public string RecipientPhone { get; set; }
    public string CompanyName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? StaffId { get; set; }
    public Guid CustomerId { get; set; }
    public int ItemCount { get; set; }
}
