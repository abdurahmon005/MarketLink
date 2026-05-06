using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Order;

namespace MarketLink.Application.Service
{
    public interface IShopOrderService
    {
        /// <summary>Savat → korxona bo'yicha guruhlab → Order[] yaratish</summary>
        Task<(bool Success, string Message, List<ShopOrderDto>? Orders)> CheckoutAsync(
            int shopId, CheckoutDto dto, CancellationToken ct = default);

        Task<PagedResult<ShopOrderGroupDto>> GetMyOrdersAsync(
            int shopId, ShopOrderFilterDto filter, CancellationToken ct = default);

        Task<ShopOrderDto?> GetOrderDetailAsync(
            int shopId, int orderId, CancellationToken ct = default);

        Task<(bool Success, string Message)> CancelOrderAsync(
            int shopId, int orderId, CancellationToken ct = default);
    }
}
