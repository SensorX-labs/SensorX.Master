using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.SupplyChainContext.AggregateModels.SupplyRequestAggregate;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.UseCases.SupplyRequests.Commands.ProcessSupplyRequest;

public class ProcessSupplyRequestCommandHandler(
    IRepository<SupplyRequest> supplyRequestRepository,
    IUnitOfWork unitOfWork
) : IRequestHandler<ProcessSupplyRequestCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(ProcessSupplyRequestCommand request, CancellationToken cancellationToken)
    {
        var targetId = new SupplyRequestId(request.SupplyRequestId);
        var allRequests = await supplyRequestRepository.ListAsync(cancellationToken);
        var supplyRequest = allRequests.FirstOrDefault(x => x.Id == targetId);

        if (supplyRequest == null)
        {
            return Result<bool>.Failure("Yêu cầu cung ứng không tồn tại");
        }

        if (supplyRequest.Status == SupplyRequestStatus.Completed)
        {
            return Result<bool>.Failure("Yêu cầu cung ứng đã được xử lý hoàn tất trước đó");
        }

        if (request.PurchaseOptions != null)
        {
            foreach (var opt in request.PurchaseOptions)
            {
                if (opt.Quantity > 0)
                {
                    supplyRequest.AddPurchaseOption(
                        new ProductId(opt.ProductId),
                        new Quantity(opt.Quantity),
                        opt.Note ?? ""
                    );
                }
            }
        }

        if (request.CompleteRequest)
        {
            supplyRequest.Complete();
        }

        await supplyRequestRepository.Update(supplyRequest, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
