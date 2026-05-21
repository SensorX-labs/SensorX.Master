using MediatR;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Common.Exceptions;
using SensorX.Master.Domain.Contexts.QuoteContext;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Commands.Quotes.CreateDraftQuote;

public class CreateDraftQuoteCommandHandler(
    IRepository<Quote> _quoteRepository,
    IRepository<RFQ> _rfqRepository,
    IRepository<SaleStaff> _saleStaffRepository,
    IRepository<Customer> _customerRepository
) : IRequestHandler<CreateDraftQuoteCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateDraftQuoteCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var rfq = await _rfqRepository.GetByIdAsync(new RFQId(request.RFQId), cancellationToken);
            if (rfq is null)
            {
                return Result<Guid>.Failure("Không tìm thấy RFQ tương ứng");
            }
            if (rfq.StaffId is null)
            {
                return Result<Guid>.Failure("Không tìm thấy nhân viên phụ trách tương ứng");
            }

            var customerInfo = await GetCustomerInfo(rfq, request, cancellationToken);
            var saleStaff = await _saleStaffRepository.GetByIdAsync(new StaffId(rfq.StaffId), cancellationToken);
            if (saleStaff is null)
            {
                return Result<Guid>.Failure("Không tìm thấy nhân viên phụ trách tương ứng");
            }
            SenderInfo sender = new SenderInfo(saleStaff.Id, saleStaff.Name, saleStaff.Email, saleStaff.Phone);
            var quote = Quote.CreateDraft(
                new RFQId(request.RfqId),
                rfq.CustomerId,
                sender,
                customerInfo
            );

            // Add quote items
            if (request.Items != null && request.Items.Count > 0)
            {
                AddQuoteItems(quote, rfq, request.Items);
            }
            else
            {
                return Result<Guid>.Failure("Quote must have at least one item.");
            }

            await _quoteRepository.AddAsync(quote, cancellationToken);

            return Result<Guid>.Success(quote.Id.Value, "Tạo bản thảo báo giá thành công");
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }

    private static void AddQuoteItems(Quote quote, RFQ rfq, List<QuoteItemDto> items)
    {
        var mapItems = items.ToDictionary(
            x => x.ProductId,
            x => (UnitPrice: Money.FromVnd(x.UnitPrice), TaxRate: Percent.From(x.TaxRate))
        );
        foreach (var item in rfq.Items)
        {
            var mapItem = mapItems[item.ProductId];
            quote.AddItem(
                new ProductId(item.ProductId),
                item.ProductCode,
                item.Manufacturer ?? "Default",
                item.Unit,
                new Quantity((int)item.Quantity),
                mapItem.UnitPrice,
                mapItem.TaxRate
            );
        }
    }

    private async Task<CustomerInfo> GetCustomerInfo(RFQ rfq, CreateDraftQuoteCommand request, CancellationToken cancellationToken)
    {
        if (rfq.CustomerInfo is null)
        {
            var customer = await _customerRepository.GetByIdAsync(new CustomerId(rfq.CustomerId), cancellationToken);
            if (customer is null)
            {
                throw new DomainException("Không tìm thấy khách hàng tương ứng");
            }
            if (string.IsNullOrWhiteSpace(customer.CompanyName) ||
                string.IsNullOrWhiteSpace(customer.Email) ||
                string.IsNullOrWhiteSpace(customer.Address) ||
                string.IsNullOrWhiteSpace(customer.TaxCode)
            )
            {
                throw new DomainException("Thông tin doanh nghiệp của khách hàng không đầy đủ");
            }
            return new CustomerInfo(
                customer.CompanyName,
                Email.From(customer.Email),
                customer.Address,
                customer.TaxCode,
                string.IsNullOrWhiteSpace(customer.Phone) ? Phone.Create("0000000000") : Phone.From(customer.Phone)
            );
        }

        return rfq.CustomerInfo;
    }
}