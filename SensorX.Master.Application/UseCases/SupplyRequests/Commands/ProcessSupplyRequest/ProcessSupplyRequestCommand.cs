using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.UseCases.SupplyRequests.Commands.ProcessSupplyRequest;

public record PurchaseOptionDto(
    Guid ProductId,
    int Quantity,
    string Note
);

public record ProcessSupplyRequestCommand(
    Guid SupplyRequestId,
    List<PurchaseOptionDto> PurchaseOptions,
    bool CompleteRequest
) : IRequest<Result<bool>>;
