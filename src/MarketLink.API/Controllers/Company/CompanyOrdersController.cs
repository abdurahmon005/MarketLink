using MarketLink.API.Common;
using MarketLink.Application.Models.Order;
using MarketLink.Application.Service;
using MarketLink.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers.Company
{
    /// <summary>Company — kelgan buyurtmalarni boshqarish</summary>
    [ApiController]
    [Route("api/company/orders")]
    [Authorize(Roles = "Company")]
    public class CompanyOrdersController : ControllerBase
    {
        private readonly ICompanyOrderService _orderService;

        public CompanyOrdersController(ICompanyOrderService orderService)
            => _orderService = orderService;

        /// <summary>Kelgan buyurtmalar ro'yxati (filter bilan)</summary>
        [HttpGet("incoming")]
        public async Task<IActionResult> GetIncoming(
            [FromQuery] OrderStatus? status,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string? shopName,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            CancellationToken ct = default)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var filter = new IncomingOrderFilter
            {
                Status   = status,
                DateFrom = dateFrom,
                DateTo   = dateTo,
                ShopName = shopName,
                Page     = page,
                PageSize = size
            };

            var result = await _orderService.GetIncomingOrdersAsync(companyId.Value, filter, ct);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Kelgan buyurtmalar",
                Data    = result
            });
        }

        /// <summary>Buyurtma tafsilotlari</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var order = await _orderService.GetOrderByIdAsync(id, companyId.Value, ct);
            if (order == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Buyurtma topilmadi yoki sizga tegishli emas"
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Buyurtma",
                Data    = order
            });
        }

        /// <summary>Buyurtma statusini o'zgartirish</summary>
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            [FromBody] UpdateOrderStatusRequest request,
            CancellationToken ct)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var (success, message) = await _orderService.UpdateStatusAsync(
                id, companyId.Value, request.Status, ct);

            if (!success)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = message
                });

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        private int? GetProfileId()
        {
            var v = User.FindFirstValue("profile_id");
            return int.TryParse(v, out var id) ? id : null;
        }
    }
}
