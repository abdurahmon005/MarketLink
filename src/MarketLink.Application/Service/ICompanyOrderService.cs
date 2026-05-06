using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Order;
using MarketLink.Domain.Enums;

namespace MarketLink.Application.Service
{
    public interface ICompanyOrderService
    {
        /// <summary>Kelgan buyurtmalar ro'yxati (filter + sahifalash)</summary>
        Task<PagedResult<OrderResponse>> GetIncomingOrdersAsync(
            int companyId, IncomingOrderFilter filter, CancellationToken ct = default);

        /// <summary>Buyurtma tafsilotlari (ownership tekshiruvi bilan)</summary>
        Task<OrderResponse?> GetOrderByIdAsync(
            int orderId, int companyId, CancellationToken ct = default);

        /// <summary>Buyurtma statusini o'zgartirish</summary>
        Task<(bool Success, string Message)> UpdateStatusAsync(
            int orderId, int companyId, OrderStatus newStatus, CancellationToken ct = default);
    }
}
