using MarketLink.API.Common;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/supplier/dashboard")]
    [Authorize(Roles = "Company,Admin")]
    public class SupplierDashboardController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public SupplierDashboardController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(
            [FromQuery] string period = "week", CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null)
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Token noto'g'ri" });

            var stats = await _statisticsService.GetSupplierStatsAsync(companyId.Value, period, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Dashboard statistikasi", Data = stats });
        }

        [HttpGet("revenue-chart")]
        public async Task<IActionResult> GetRevenueChart(
            [FromQuery] string period = "week", CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null)
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Token noto'g'ri" });

            var chart = await _statisticsService.GetRevenueChartAsync(companyId.Value, period, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Daromad grafigi", Data = chart });
        }

        [HttpGet("top-buyers")]
        public async Task<IActionResult> GetTopBuyers(
            [FromQuery] string period = "week", CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null)
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Token noto'g'ri" });

            var buyers = await _statisticsService.GetTopBuyersAsync(companyId.Value, period, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Top xaridorlar", Data = buyers });
        }

        [HttpGet("activity")]
        public async Task<IActionResult> GetActivity(
            [FromQuery] int limit = 20, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null)
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Token noto'g'ri" });

            var activity = await _statisticsService.GetRecentActivityAsync(companyId.Value, limit, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "So'nggi faoliyat", Data = activity });
        }

        private int? GetCompanyId()
        {
            var value = User.FindFirstValue("profile_id");
            return int.TryParse(value, out var id) ? id : null;
        }
    }
}
