using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Company;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/company/profile")]
    [Authorize(Roles = "Company,Admin")]
    public class CompanyProfileController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyProfileController(ICompanyService companyService)
            => _companyService = companyService;

        /// <summary>Korxona profilini olish</summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile(CancellationToken ct)
        {
            var userId = GetUserId();
            if (userId == null) return Forbid();

            var profile = await _companyService.GetByUserIdAsync(userId.Value, ct);
            if (profile == null)
                return NotFound(ApiResponse<object>.Fail("Profil topilmadi"));

            return Ok(ApiResponse<CompanyProfileResponse>.Ok(profile, "Korxona profili"));
        }

        /// <summary>Korxona profilini yangilash</summary>
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateCompanyProfileRequest request, CancellationToken ct)
        {
            var userId = GetUserId();
            if (userId == null) return Forbid();

            var (success, message) = await _companyService.UpdateAsync(userId.Value, request, ct);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(message));

            return Ok(ApiResponse<object>.Ok(null!, message));
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private Guid? GetUserId()
        {
            var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(v, out var id) ? id : null;
        }
    }
}
