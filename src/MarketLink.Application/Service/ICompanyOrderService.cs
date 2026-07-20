using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Order;
using MarketLink.Application.Models.Supplier;
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

        // ── Supplier Panel ────────────────────────────────────────────────────

        Task<PagedResult<SupplierOrderListDto>> GetOrdersAsync(
            int companyId, SupplierOrderFilter filter, CancellationToken ct = default);

        Task<SupplierOrderDetailDto?> GetOrderDetailAsync(
            int orderId, int companyId, CancellationToken ct = default);

        Task<(bool Success, string Message)> AcceptOrderAsync(
            int orderId, int companyId, Guid acceptedBy, CancellationToken ct = default);

        Task<(bool Success, string Message)> RejectOrderAsync(
            int orderId, int companyId, string reason, Guid rejectedBy, CancellationToken ct = default);

        Task<(bool Success, string Message)> AssignDriverAsync(
            int orderId, int companyId, AssignDriverRequest request, Guid assignedBy, CancellationToken ct = default);

        Task<int> GetNewOrdersCountAsync(int companyId, CancellationToken ct = default);
    }
}
