using MediatR;
using SensorX.Master.Application.Common.Interfaces;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Application.Common.ResponseClient;
using SensorX.Master.Domain.Contexts.QuoteContext.AggregateModels.QuoteAggregate;
using SensorX.Master.Domain.SeedWork;

namespace SensorX.Master.Application.Commands.Quotes.DeleteQuote;

public sealed class DeleteQuoteHandler(
    IRepository<Quote> _quoteRepository,
    IQueryBuilder<SaleStaff> _saleStaffQueryBuilder,
    IQueryExecutor _queryExecutor,
    ICurrentUser _currentUser
) : IRequestHandler<DeleteQuoteCommand, Result>
{
    // <sumary>
    // Nhân viên xóa báo giá nháp
    // </summary>
    public async Task<Result> Handle(DeleteQuoteCommand request, CancellationToken cancellationToken)
    {
        var quote = await _quoteRepository.GetByIdAsync(new QuoteId(request.Id), cancellationToken);
        if (quote is null)
            return Result.Failure("Báo giá không tồn tại");
        if (quote.Status != QuoteStatus.Draft)
            return Result.Failure("Không thể xóa báo giá không ở trạng thái nháp");

        var staffId = await _queryExecutor.FirstOrDefaultAsync(
            _saleStaffQueryBuilder.QueryAsNoTracking.Where(x => x.AccountId == _currentUser.UserId).Select(x => x.Id)
        , cancellationToken);
        if (staffId is null)
            return Result.Failure("Nhân viên không tồn tại");

        // Chỉ có SaleStaff mới được xóa
        if (quote.SenderInfo.Id != staffId)
            return Result.Failure("Chỉ có người tạo mới có thể xóa báo giá");

        await _quoteRepository.DeleteAsync(quote, cancellationToken);
        return Result.Success("Xóa báo giá thành công");
    }
}