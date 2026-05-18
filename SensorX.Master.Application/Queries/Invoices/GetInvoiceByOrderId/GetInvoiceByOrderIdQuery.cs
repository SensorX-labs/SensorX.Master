using MediatR;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Application.Queries.Invoices.GetInvoiceById;

namespace SensorX.Master.Application.Queries.Invoices.GetInvoiceByOrderId;

public record GetInvoiceByOrderIdQuery(Guid OrderId) : IRequest<Result<GetInvoiceByIdResponse>>;
