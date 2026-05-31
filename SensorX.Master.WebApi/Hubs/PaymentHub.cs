using Microsoft.AspNetCore.SignalR;

namespace SensorX.Master.WebApi.Hubs;

public class PaymentHub : Hub
{
    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        // Store the order ID in the connection metadata if provided
        var orderId = Context.GetHttpContext()?.Request.Query["orderId"].ToString();
        if (!string.IsNullOrEmpty(orderId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Method to subscribe a client to payment updates for a specific order.
    /// Called from the frontend when the order detail page loads.
    /// </summary>
    public async Task SubscribeToOrderPaymentUpdates(string orderId)
    {
        if (string.IsNullOrEmpty(orderId))
            return;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }

    /// <summary>
    /// Method to unsubscribe a client from payment updates for a specific order.
    /// Called from the frontend when leaving the order detail page.
    /// </summary>
    public async Task UnsubscribeFromOrderPaymentUpdates(string orderId)
    {
        if (string.IsNullOrEmpty(orderId))
            return;

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
    }
}
