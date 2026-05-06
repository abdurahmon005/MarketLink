using FluentValidation;
using MarketLink.API.Common;
using MarketLink.Application.Models.Company;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers.Company
{
    /// <summary>Company profil boshqaruvi</summary>
    [ApiController]
    [Route("api/company")]
    [Authorize(Roles = "Company")]
    public class CompanyProfileController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly IValidator<UpdateCompanyProfileRequest> _validator;

        public CompanyProfileController(
            ICompanyService companyService,
            IValidator<UpdateCompanyProfileRequest> validator)
        {
            _companyService = companyService;
            _validator      = validator;
        }

        /// <summary>O'z kompaniya profilini ko'rish</summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile(CancellationToken ct)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var company = await _companyService.GetByUserIdAsync(userId.Value, ct);
            if (company == null)
                return NotFound(Fail("Kompaniya profili topilmadi"));

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Kompaniya profili",
                Data    = company
            });
        }

        /// <summary>Kompaniya profilini yangilash</summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateCompanyProfileRequest request,
            CancellationToken ct)
        {
            var validation = await _validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Validatsiya xatosi",
                    Errors  = validation.Errors.Select(e => e.ErrorMessage).ToList()
                });

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var (success, message) = await _companyService.UpdateAsync(userId.Value, request, ct);

            if (!success)
                return BadRequest(Fail(message));

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        private Guid? GetUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }

        private static ApiResponse<object> Fail(string msg) =>
            new() { Success = false, Message = msg };
    }
}
