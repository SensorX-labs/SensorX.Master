using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.RFQAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.ValueObjects;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.Common.Exceptions;
using MediatR;

namespace SensorX.Master.Application.Commands.RFQs.CreateRFQ
{
    public class CreateRFQCommandHandler(
        IRepository<RFQ> _rfqRepository 
    ) : IRequestHandler<CreateRFQCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(CreateRFQCommand request, CancellationToken cancellationToken)
        {
            if (request.Items == null || !request.Items.Any())
            {
                return Result<Guid>.Failure("Danh sách sản phẩm không được để trống.");
            }

            // Gộp số lượng nếu trùng sản phẩm
            var groupedItems = request.Items
                .GroupBy(i => i.ProductId)
                .Select(g => new {
                    ProductId = g.Key,
                    Info = g.First(),
                    TotalQuantity = g.Sum(x => x.Quantity)
                });

            var customerInfo = new CustomerInfo(
                request.RecipientName,
                Phone.From(request.RecipientPhone),
                request.CompanyName,
                Email.From(request.Email),
                request.Address,
                request.TaxCode
            );

            var rfq = new RFQ(
                RFQId.New(),
                Code.Create("RFQ"),
                null,
                new CustomerId(request.CustomerId),
                customerInfo,
                RFQStatus.Pending
            );

            try 
            {
                foreach (var group in groupedItems)
                {
                    var itemDto = group.Info;
                    // RFQItem ở đây là Domain Entity
                    var rfqItem = new RFQItem(
                        RFQItemId.New(),
                        new ProductId(group.ProductId),
                        itemDto.ProductName,
                        new Quantity(group.TotalQuantity),
                        Code.From(itemDto.ProductCode),
                        itemDto.Manufacturer,
                        itemDto.Unit
                    );
                    rfq.AddItem(rfqItem);
                }

                await _rfqRepository.Add(rfq, cancellationToken);
                await _rfqRepository.SaveChangesAsync(cancellationToken);
                
                return Result<Guid>.Success(rfq.Id.Value);
            }
            catch (DomainException ex)
            {
                return Result<Guid>.Failure($"Dữ liệu không hợp lệ: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Result<Guid>.Failure($"Lỗi hệ thống: {ex.Message}");
            }
        }
    }
}