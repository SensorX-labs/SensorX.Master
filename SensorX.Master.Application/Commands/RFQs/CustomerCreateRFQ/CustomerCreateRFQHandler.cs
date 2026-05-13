using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Common.Exceptions;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Commands.RFQs.CustomerCreateRFQ;

public class CustomerCreateRFQCommandHandler(
    IRepository<RFQ> _rfqRepository,
    IQueryBuilder<Customer> _customerBuilder,
    IQueryBuilder<Product> _productBuilder,
    IQueryExecutor _queryExecutor,
    ICurrentUser _currentUser
) : IRequestHandler<CustomerCreateRFQCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CustomerCreateRFQCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return Result<Guid>.Failure("Danh sách sản phẩm không được để trống.");
        }

        // Gộp số lượng nếu trùng sản phẩm
        var itemQuantities = request.Items
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        var customerQuery = _customerBuilder.QueryAsNoTracking.Where(c => c.AccountId == _currentUser.UserId);
        var customer = await _queryExecutor.FirstOrDefaultAsync(customerQuery, cancellationToken);
        if (customer == null)
        {
            return Result<Guid>.Failure("Không tìm thấy khách hàng.");
        }

        if (customer.Phone == null || customer.Address == null)
        {
            return Result<Guid>.Failure("Thông tin khách hàng không đầy đủ. Vui lòng bổ sung hồ sơ !");
        }

        var customerInfo = new CustomerInfo(
            customer.CompanyName,
            customer.Phone,
            customer.CompanyName,
            customer.Email,
            customer.Address,
            customer.TaxCode
        );

        var rfq = new RFQ(
            RFQId.New(),
            Code.Create("RFQ"),
            null,
            new CustomerId(customer.Id),
            customerInfo
        );

        var productIds = itemQuantities.Keys.ToList(); // convert to list for query in EF
        var productQuery = _productBuilder.QueryAsNoTracking.Where(c => productIds.Contains(c.Id));
        var productList = await _queryExecutor.ToListAsync(productQuery, cancellationToken);

        foreach (var product in productList)
        {
            rfq.AddItem(
                new ProductId(product.Id),
                product.Name,
                new Quantity(itemQuantities[product.Id]),
                Code.From(product.Code),
                product.Manufacturer,
                product.Unit
            );
        }

        await _rfqRepository.AddAsync(rfq, cancellationToken);

        return Result<Guid>.Success(rfq.Id.Value, "Thêm sản phẩm vào RFQ thành công.");
    }
}