using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Common.ReadModel
{
    public class Product : IAggregateRoot
    {
        public Product(
            ProductId id,
            Code code,
            string name,
            string manufacturer,
            string unit,
            ProductStatus status,
            DateTimeOffset createdAt
        )
        {
            Id = id;
            Name = name;
            Code = code;
            Manufacturer = manufacturer;
            Unit = unit;
            Status = status;
            CreatedAt = createdAt;
        }
        public ProductId Id { get; }
        public Code Code { get; }
        public string Name { get; private set; }
        public string Manufacturer { get; private set; }
        public string Unit { get; private set; }
        public ProductStatus Status { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public DateTimeOffset? UpdatedAt { get; private set; }

        public void Update(
            string name,
            string manufacturer,
            string unit,
            DateTimeOffset? updatedAt
        )
        {
            Name = name;
            Manufacturer = manufacturer;
            Unit = unit;
            UpdatedAt = updatedAt;
        }

        public void ChangeStatus(
            ProductStatus status,
            DateTimeOffset? updatedAt
        )
        {
            Status = status;
            UpdatedAt = updatedAt;
        }
    }

    public enum ProductStatus
    {
        Active, // Đang kinh doanh
        Inactive // Ngừng kinh doanh
    }
}