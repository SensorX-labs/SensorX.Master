using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Commands.RFQs.CustomerAddProduct;

public class CustomerAddProductCommandHandler(
    IRepository<RFQ> _rfqRepository,
    IDataServiceClient _dataServiceClient
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

        var productIds = itemQuantities.Keys.ToArray();
        var productList = await _dataServiceClient.GetProductPricingAsync(productIds);

        foreach (var product in productList)
        {
            rfq.AddItem(
                new ProductId(product.ProductId),
                product.ProductName,
                new Quantity(itemQuantities[product.ProductId]),
                Code.From(product.ProductCode),
                product.Manufacture,
                product.Unit
            );
        }

        await _rfqRepository.SaveChangesAsync(cancellationToken);

        return Result.Success("Thêm sản phẩm vào RFQ thành công.");
    }
}