using MediatR;
using SensorX.Master.Application.Common.ResponseClient;

namespace SensorX.Master.Application.Commands.Quotes.SubmitQuoteForApproval;

public record SubmitQuoteForApprovalCommand(Guid QuoteId) : IRequest<Result>;
