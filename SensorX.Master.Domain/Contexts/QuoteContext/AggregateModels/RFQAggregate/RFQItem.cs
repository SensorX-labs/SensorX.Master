using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate
{
    public class RFQItem : Entity<RFQItemId>
    
    {
        private RFQItem() : base() { }

        public RFQItem(RFQItemId id, ProductId productId, string productName, Quantity quantity, Code productCode, string manufacturer, string unit) : base(id)
        {
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            ProductCode = productCode;
            Manufacturer = manufacturer;
            Unit = unit;
        }
        public ProductId ProductId { get; private set; }
        public string ProductName { get; private set; }
        public Quantity Quantity { get; private set; }
        public Code ProductCode { get; private set; }
        public string? Manufacturer { get; private set; }
        public string Unit { get; private set; }
    }
}