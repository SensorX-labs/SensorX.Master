using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate
{
    public class QuoteItem : Entity<QuoteItemId>
    {
        private QuoteItem() : base() { }

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

        public ProductId ProductId { get; set; }
        public Code ProductCode { get; set; }
        public string Manufacturer { get; set; }
        public string Unit { get; set; }
        public Quantity Quantity { get; set; }
        public Money UnitPrice { get; set; }
        public Percent TaxRate { get; set; }

        public Money GetLineAmount() => UnitPrice * Quantity;
        public Money GetTaxAmount() => GetLineAmount() * TaxRate;
        public Money GetTotalLineAmount() => GetLineAmount() + GetTaxAmount();
    }
}

