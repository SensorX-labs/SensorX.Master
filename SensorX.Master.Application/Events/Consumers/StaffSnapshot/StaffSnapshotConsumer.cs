using MassTransit;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Events.Consumers.StaffSnapshot;

public class StaffSnapshotConsumer(
    IRepository<SaleStaff> _staffRepository
) : IConsumer<CreateStaffEvent>,
    IConsumer<UpdateStaffEvent>
{
    public async Task Consume(ConsumeContext<CreateStaffEvent> context)
    {
        var staffEvent = context.Message;
        if (staffEvent.Department != Department.Sale) return;
        var staff = new SaleStaff(
            new StaffId(staffEvent.Id),
            new AccountId(staffEvent.AccountId),
            Code.From(staffEvent.Code),
            staffEvent.Name,
            Email.From(staffEvent.Email),
            null
        );
        await _staffRepository.AddAsync(staff, context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<UpdateStaffEvent> context)
    {
        var staffEvent = context.Message;
        var staff = await _staffRepository.GetByIdAsync(new StaffId(staffEvent.Id), context.CancellationToken);
        if (staff == null) return;

        staff.Update(
            staffEvent.Name,
            Email.From(staffEvent.Email),
            staffEvent.Phone != null ? Phone.From(staffEvent.Phone) : null
        );
        await _staffRepository.SaveChangesAsync(context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<UpdateStaffAvatarEvent> context)
    {
        var staffEvent = context.Message;
        var staff = await _staffRepository.GetByIdAsync(new StaffId(staffEvent.Id), context.CancellationToken);
        if (staff == null) return;

        staff.UpdateAvatarUrl(staffEvent.AvatarUrl);
        await _staffRepository.SaveChangesAsync(context.CancellationToken);
    }
}