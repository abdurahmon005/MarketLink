using FluentValidation;
using MarketLink.API.Common;
using MarketLink.Application.Models.Shop;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers.Shop
{
    /// <summary>Do'kon profil boshqaruvi</summary>
    [ApiController]
    [Route("api/shop")]
    [Authorize(Roles = "Shop")]
    public class ShopProfileController : ControllerBase
    {
        private readonly IShopProfileService _profileService;
        private readonly IValidator<UpdateShopProfileRequest> _validator;

        public ShopProfileController(
            IShopProfileService profileService,
            IValidator<UpdateShopProfileRequest> validator)
        {
            _profileService = profileService;
            _validator      = validator;
        }

        /// <summary>O'z profil ma'lumotlarini ko'rish</summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile(CancellationToken ct)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var profile = await _profileService.GetProfileAsync(userId.Value, ct);
            if (profile == null)
                return NotFound(Fail("Do'kon profili topilmadi"));

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Do'kon profili",
                Data    = profile
            });
        }

        /// <summary>Profil ma'lumotlarini yangilash</summary>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateShopProfileRequest request, CancellationToken ct)
        {
            var validation = await _validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return BadRequest(ValidationFail(validation));

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var (success, message) = await _profileService.UpdateProfileAsync(userId.Value, request, ct);

            if (!success) return BadRequest(Fail(message));
            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        /// <summary>Logo va guvohnomani qayta yuklash</summary>
        [HttpPut("profile/media")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateMedia(
            [FromForm] UpdateShopMediaRequest request,
            CancellationToken ct)
        {
            if (request.Logo == null && request.Certificate == null)
                return BadRequest(Fail("Kamida bitta fayl yuklang"));

            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var (success, message) =
                await _profileService.UpdateMediaAsync(userId.Value, request.Logo, request.Certificate, ct);

            if (!success) return BadRequest(Fail(message));
            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        private Guid? GetUserId()
        {
            var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(v, out var id) ? id : null;
        }

        private static ApiResponse<object> Fail(string msg) =>
            new() { Success = false, Message = msg };

        private static ApiResponse<object> ValidationFail(
            FluentValidation.Results.ValidationResult v) => new()
        {
            Success = false,
            Message = "Validatsiya xatosi",
            Errors  = v.Errors.Select(e => e.ErrorMessage).ToList()
        };
    }
}
