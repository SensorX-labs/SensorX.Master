using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Invoices.GetInvoiceById;

public record GetInvoiceByIdQuery(Guid InvoiceId) : IRequest<Result<GetInvoiceByIdResponse>>;
