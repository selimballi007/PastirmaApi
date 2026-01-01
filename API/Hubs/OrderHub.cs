using Microsoft.AspNetCore.SignalR;

namespace PastirmaApi.API.Hubs
{
    public class OrderHub : Hub
    {
        public async Task SendOrderNotification(string message)
        {
            await Clients.All.SendAsync("ReceiveOrderNotification", message);
        }

        public async Task NotifyNewOrder(object orderData)
        {
            // Send notification to all connected admin clients
            await Clients.All.SendAsync("NewOrderCreated", orderData);
        }

        public async Task NotifyOrderStatusChanged(int orderId, string status)
        {
            // Notify all clients about order status change
            await Clients.All.SendAsync("OrderStatusChanged", new
            {
                orderId,
                status,
                timestamp = DateTime.UtcNow
            });
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        }
    }
}
