using System.Linq;
using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Commands.RFQs.CustomerSendRFQ;

public class CustomerSendRFQCommandHandler(
    IRepository<RFQ> _rfqRepository,
    IQueryBuilder<Customer> _customerBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<CustomerSendRFQCommand, Result>
{
    public async Task<Result> Handle(CustomerSendRFQCommand request, CancellationToken cancellationToken)
    {
        var rfq = await _rfqRepository.GetByIdAsync(new RFQId(request.Id), cancellationToken);
        if (rfq is null)
        {
            return Result.Failure("Không tìm thấy RFQ.");
        }

        var customerQuery = _customerBuilder.QueryAsNoTracking.Where(c => c.Id == rfq.CustomerId);
        var customer = await _queryExecutor.FirstOrDefaultAsync(customerQuery, cancellationToken);
        if (customer == null)
        {
            return Result.Failure("Không tìm thấy khách hàng liên kết với yêu cầu báo giá này.");
        }

        if (string.IsNullOrWhiteSpace(customer.CompanyName) ||
            string.IsNullOrWhiteSpace(customer.Email.Value) ||
            string.IsNullOrWhiteSpace(customer.Address) ||
            string.IsNullOrWhiteSpace(customer.TaxCode)
        )
        {
            return Result.Failure("Thông tin doanh nghiệp của khách hàng không đầy đủ. Vui lòng bổ sung thông tin công ty!");
        }

        var customerInfo = new CustomerInfo(
            customer.CompanyName,
            customer.Email,
            customer.Address,
            customer.TaxCode,
            customer.Phone ?? Phone.Create("0000000000")
        );

        rfq.Send(customerInfo);
        await _rfqRepository.SaveChangesAsync(cancellationToken);
        return Result.Success("Gửi yêu cầu báo giá thành công");
    }
}