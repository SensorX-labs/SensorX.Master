using MassTransit;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Events.Consumers.ProductSnapshot;

public class ProductSnapshotConsumer(
    IRepository<Product> _productRepository
) : IConsumer<CreateProductEvent>,
    IConsumer<UpdateProductEvent>,
    IConsumer<ChangeProductStatusEvent>,
    IConsumer<DeleteProductEvent>
{
    public async Task Consume(ConsumeContext<CreateProductEvent> context)
    {
        var productEvent = context.Message;
        var product = new Product(
            new ProductId(productEvent.Id),
            Code.From(productEvent.Code),
            productEvent.Name,
            productEvent.Manufacture,
            productEvent.Unit,
            productEvent.Status,
            productEvent.CreatedAt
        );

        await _productRepository.AddAsync(product, context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<UpdateProductEvent> context)
    {
        var productEvent = context.Message;
        var product = await _productRepository.GetByIdAsync(new ProductId(productEvent.Id), context.CancellationToken);
        if (product == null) return;

        product.Update(
            productEvent.Name,
            productEvent.Manufacture,
            productEvent.Unit,
            productEvent.UpdatedAt
        );

        await _productRepository.SaveChangesAsync(context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<ChangeProductStatusEvent> context)
    {
        var productEvent = context.Message;
        var product = await _productRepository.GetByIdAsync(new ProductId(productEvent.Id), context.CancellationToken);
        if (product == null) return;

        product.ChangeStatus(
            productEvent.Status,
            productEvent.UpdatedAt
        );

        await _productRepository.SaveChangesAsync(context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<DeleteProductEvent> context)
    {
        var productEvent = context.Message;
        var product = await _productRepository.GetByIdAsync(new ProductId(productEvent.Id), context.CancellationToken);
        if (product == null) return;

        await _productRepository.DeleteAsync(product, context.CancellationToken);
    }
}