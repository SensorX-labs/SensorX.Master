using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate
{
    public class QuoteItem : Entity<QuoteItemId>
    {
        public QuoteItem(
            QuoteItemId id,
            ProductId productId,
            Code productCode,
            string manufacturer,
            string unit,
            Quantity quantity,
            Money unitPrice,
            Percent taxRate
        ) : base(id)
        {
            ProductId = productId;
            ProductCode = productCode;
            Manufacturer = manufacturer;
            Unit = unit;
            Quantity = quantity;
            UnitPrice = unitPrice;
            TaxRate = taxRate;
        }

        public ProductId ProductId { get; private set; }
        public Code ProductCode { get; private set; }
        public string Manufacturer { get; private set; }
        public string Unit { get; private set; }
        public Quantity Quantity { get; private set; }
        public Money UnitPrice { get; private set; }
        public Percent TaxRate { get; private set; }

        public Money GetLineAmount() => UnitPrice * Quantity;
        public Money GetTaxAmount() => GetLineAmount() * TaxRate;
        public Money GetTotalLineAmount() => GetLineAmount() + GetTaxAmount();
    }
}
