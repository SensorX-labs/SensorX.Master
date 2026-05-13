using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Common.Exceptions;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Commands.RFQs.CustomerAddProduct;

public class CustomerAddProductCommandHandler(
    IRepository<RFQ> _rfqRepository,
    IQueryBuilder<Product> _productBuilder,
    IQueryExecutor _queryExecutor
) : IRequestHandler<CustomerAddProductCommand, Result>
{
    public async Task<Result> Handle(CustomerAddProductCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            return Result.Failure("Danh sách sản phẩm không được để trống.");
        }

        var rfq = await _rfqRepository.GetByIdAsync(new RFQId(request.Id), cancellationToken);
        if (rfq is null)
        {
            return Result.Failure("Không tìm thấy RFQ.");
        }

        if (rfq.Status != RFQStatus.Draft)
        {
            return Result.Failure("RFQ đang ở trạng thái không hợp lệ.");
        }

        // Gộp số lượng nếu trùng sản phẩm
        var itemQuantities = request.Items
            .GroupBy(i => i.ProductId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

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

        await _rfqRepository.SaveChangesAsync(cancellationToken);

        return Result.Success("Thêm sản phẩm vào RFQ thành công.");
    }
}