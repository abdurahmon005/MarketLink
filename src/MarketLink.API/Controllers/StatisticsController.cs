using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Statistics;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/statistics")]
    [Authorize(Roles = "Company,Admin")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
            => _statisticsService = statisticsService;

        /// <summary>Kunlik statistika</summary>
        [HttpGet("daily")]
        public async Task<IActionResult> GetDaily(
            [FromQuery] DateTime? date = null, CancellationToken ct = default)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var targetDate = date ?? DateTime.UtcNow.Date;
            var result = await _statisticsService.GetDailyAsync(companyId.Value, targetDate, ct);
            return Ok(ApiResponse<DailyStatisticsResponse>.Ok(result, "Kunlik statistika"));
        }

        /// <summary>Oylik statistika</summary>
        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthly(
            [FromQuery] int? year = null,
            [FromQuery] int? month = null,
            CancellationToken ct = default)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var now = DateTime.UtcNow;
            var y   = year  ?? now.Year;
            var m   = month ?? now.Month;

            var result = await _statisticsService.GetMonthlyAsync(companyId.Value, y, m, ct);
            return Ok(ApiResponse<MonthlyStatisticsResponse>.Ok(result, "Oylik statistika"));
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private int? GetProfileId()
        {
            var v = User.FindFirstValue("profile_id");
            return int.TryParse(v, out var id) ? id : null;
        }
    }
}
