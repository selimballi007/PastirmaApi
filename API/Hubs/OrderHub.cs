using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace PastirmaApi.API.Hubs
{
    public class OrderHub : Hub
    {
        // No public methods needed - notifications triggered from services via IHubContext

        public override async Task OnConnectedAsync()
        {
            // Get user role from claims
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            Console.WriteLine($"Client connected: {Context.ConnectionId}, Role: {userRole ?? "Anonymous"}");

            // Add to Admin group if user is an Admin
            if (userRole == "Admin")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admin");
                Console.WriteLine($"Admin user added to Admin group: {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Get user role from claims
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

            // Remove from Admin group if user is an Admin
            if (userRole == "Admin")
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admin");
                Console.WriteLine($"Admin user removed from Admin group: {Context.ConnectionId}");
            }

            await base.OnDisconnectedAsync(exception);
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        }
    }
}
