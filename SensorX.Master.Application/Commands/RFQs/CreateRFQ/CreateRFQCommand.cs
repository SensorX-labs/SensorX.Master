using SensorX.Master.Application.Common.ResponseClient;
using MediatR;
using System.Collections.Generic;
using SensorX.Master.Application.Common.Dtos.Requests.RFQs;

namespace SensorX.Master.Application.Commands.RFQs.CreateRFQ
{
    public class CreateRFQCommand : IRequest<Result<Guid>>
    {
        public Guid StaffId { get; set; }
        public Guid CustomerId { get; set; }
        
        // flat customer info
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string TaxCode { get; set; }

        public List<RFQItem> Items { get; set; }
    }
}