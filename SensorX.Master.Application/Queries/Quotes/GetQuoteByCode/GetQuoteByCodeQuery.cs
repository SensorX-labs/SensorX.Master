using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Quotes.GetQuoteByCode;

public record GetQuoteByCodeQuery(string Code) : IRequest<Result<GetQuoteByCodeResponse>>;
