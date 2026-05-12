using MassTransit;
using SensorX.Master.Application.Common.ReadModel;
using SensorX.Master.Domain.SeedWork;
using SensorX.Master.Domain.StrongIDs;
using SensorX.Master.Domain.ValueObjects;

namespace SensorX.Master.Application.Events.Consumers.CustomerSnapshot;

public class CustomerSnapshotConsumer(
    IRepository<Customer> _customerRepository
) : IConsumer<CreateCustomerEvent>,
    IConsumer<UpdateCustomerInfoEvent>,
    IConsumer<UpdateShippingInfoEvent>
{
    public async Task Consume(ConsumeContext<CreateCustomerEvent> context)
    {
        var customerEvent = context.Message;
        var customer = new Customer(
            new CustomerId(customerEvent.Id),
            new AccountId(customerEvent.AccountId),
            customerEvent.CompanyName,
            customerEvent.TaxCode,
            Email.From(customerEvent.Email),
            string.IsNullOrEmpty(customerEvent.Phone) ? null : Phone.From(customerEvent.Phone),
            customerEvent.Address,
            customerEvent.CreatedAt
        );

        await _customerRepository.AddAsync(customer, context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<UpdateCustomerInfoEvent> context)
    {
        var customerEvent = context.Message;
        var customer = await _customerRepository.GetByIdAsync(new CustomerId(customerEvent.Id), context.CancellationToken);
        if (customer == null)
            return;

        customer.Update(
            customerEvent.Name,
            Email.From(customerEvent.Email),
            string.IsNullOrEmpty(customerEvent.Phone) ? null : Phone.From(customerEvent.Phone),
            customerEvent.Address,
            customerEvent.TaxCode,
            customerEvent.UpdatedAt
        );
        await _customerRepository.SaveChangesAsync(context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<UpdateShippingInfoEvent> context)
    {
        var customerEvent = context.Message;
        var customer = await _customerRepository.GetByIdAsync(new CustomerId(customerEvent.Id), context.CancellationToken);
        if (customer == null)
            return;

        customer.UpdateShippingInfo(
            string.IsNullOrEmpty(customerEvent.ReceiverPhone) ? null : Phone.From(customerEvent.ReceiverPhone),
            customerEvent.ReceiverName,
            customerEvent.ShippingAddress,
            customerEvent.UpdatedAt
        );
        await _customerRepository.SaveChangesAsync(context.CancellationToken);
    }

}