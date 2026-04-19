using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Queries.Quotes.GetDetailQuoteById;

public class GetDetailQuoteByIdQuery : IRequest<Result<GetDetailQuoteByIdResponse>>
{
    public Guid QuoteId { get; set; }

    public GetDetailQuoteByIdQuery(Guid quoteId)
    {
        QuoteId = quoteId;
    }
}
