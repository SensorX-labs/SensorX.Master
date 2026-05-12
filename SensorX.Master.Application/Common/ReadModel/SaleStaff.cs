using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Common.ReadModel
{
    public class SaleStaff : IAggregateRoot
    {
        public SaleStaff(StaffId id, AccountId accountId, Code code, string name, Email email, Phone? phone)
        {
            Id = id;
            AccountId = accountId;
            Code = code;
            Name = name;
            Email = email;
            Phone = phone;
        }

        public StaffId Id { get; }
        public AccountId AccountId { get; }
        public Code Code { get; }
        public string Name { get; private set; }
        public Email Email { get; private set; }
        public Phone? Phone { get; private set; }

        public void Update(
            string name,
            Email email,
            Phone? phone
        )
        {
            Name = name;
            Email = email;
            Phone = phone;
        }
    }
}