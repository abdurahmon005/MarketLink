using MarketLink.API.Common;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers.Company
{
    /// <summary>Company statistika va analitika</summary>
    [ApiController]
    [Route("api/statistics")]
    [Authorize(Roles = "Company")]
    public class CompanyStatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public CompanyStatisticsController(IStatisticsService statisticsService)
            => _statisticsService = statisticsService;

        /// <summary>Kunlik statistika</summary>
        [HttpGet("daily")]
        public async Task<IActionResult> GetDaily(
            [FromQuery] DateTime? date,
            CancellationToken ct)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var targetDate = date ?? DateTime.UtcNow.Date;
            var result = await _statisticsService.GetDailyAsync(companyId.Value, targetDate, ct);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"{targetDate:dd.MM.yyyy} kunlik statistika",
                Data    = result
            });
        }

        /// <summary>Oylik statistika</summary>
        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthly(
            [FromQuery] int? year,
            [FromQuery] int? month,
            CancellationToken ct)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var now         = DateTime.UtcNow;
            var targetYear  = year  ?? now.Year;
            var targetMonth = month ?? now.Month;

            if (targetMonth < 1 || targetMonth > 12)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Oy 1 dan 12 gacha bo'lishi kerak"
                });

            var result = await _statisticsService.GetMonthlyAsync(
                companyId.Value, targetYear, targetMonth, ct);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"{targetYear}/{targetMonth:D2} oylik statistika",
                Data    = result
            });
        }

        private int? GetProfileId()
        {
            var v = User.FindFirstValue("profile_id");
            return int.TryParse(v, out var id) ? id : null;
        }
    }
}
