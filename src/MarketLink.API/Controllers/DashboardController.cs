using MarketLink.API.Common;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
            => _dashboardService = dashboardService;

        /// <summary>Monthly stats, chart data and top products for the shop</summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var stats = await _dashboardService.GetStatsAsync(shopId.Value, ct);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Dashboard statistikasi",
                Data    = stats
            });
        }

        /// <summary>Orders currently in-transit for the shop</summary>
        [HttpGet("active-deliveries")]
        public async Task<IActionResult> GetActiveDeliveries(CancellationToken ct)
        {
            var shopId = GetProfileId();
            if (shopId == null) return Forbid();

            var deliveries = await _dashboardService.GetActiveDeliveriesAsync(shopId.Value, ct);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Faol yetkazib berishlar",
                Data    = deliveries
            });
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private int? GetProfileId()
        {
            var v = User.FindFirstValue("profile_id");
            return int.TryParse(v, out var id) ? id : null;
        }
    }
}
