using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MarketLink.API.Hubs
{
    [Authorize]
    public class TrackingHub : Hub
    {
        /// <summary>Join a group to receive updates for a specific order</summary>
        public async Task JoinOrderGroup(string orderId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"order_{orderId}");
        }

        /// <summary>Leave order-level group</summary>
        public async Task LeaveOrderGroup(string orderId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"order_{orderId}");
        }

        /// <summary>Join a group to receive all updates for a shop</summary>
        public async Task JoinShopGroup(string shopId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"shop_{shopId}");
        }
    }
}
