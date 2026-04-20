using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Quotes.GetDetailQuoteById;

public record GetDetailQuoteByIdQuery(Guid QuoteId) : IRequest<Result<GetDetailQuoteByIdResponse>>;
