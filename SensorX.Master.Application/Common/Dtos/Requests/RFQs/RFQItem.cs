namespace SensorX.Master.Application.Common.Dtos.Requests.RFQs
{
    public class RFQItem
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string ProductCode { get; set; }
        public string Manufacturer { get; set; }
        public string Unit { get; set; }
    }
}