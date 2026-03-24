using MarketLink.API.Attributes;
using MarketLink.API.Common;
using MarketLink.Application.Models.Company;
using MarketLink.Application.Models.Shop;
using MarketLink.Application.Service;
using MarketLink.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService          _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger      = logger;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var profile = await _userService.GetUserProfileAsync(userId.Value);
            if (profile == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Foydalanuvchi topilmadi"
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Profil",
                Data    = profile
            });
        }
        [HttpPut("me/company")]
        public async Task<IActionResult> UpdateCompanyProfile([FromBody] UpdateCompanyProfileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ValidationError());

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var (success, message) = await _userService.UpdateCompanyProfileAsync(userId.Value, request);

            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        [HttpPut("me/shop")]
        public async Task<IActionResult> UpdateShopProfile([FromBody] UpdateShopProfileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ValidationError());

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var (success, message) = await _userService.UpdateShopProfileAsync(userId.Value, request);

            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        [RequirePermission("admin.read_all")]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var profile = await _userService.GetUserProfileAsync(id);
            if (profile == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Foydalanuvchi topilmadi"
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Foydalanuvchi",
                Data    = profile
            });
        }

        [RequirePermission("admin.block")]
        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ValidationError());

            var result = await _userService.UpdateUserStatusAsync(id, request.Status);
            if (!result)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Foydalanuvchi topilmadi"
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = $"Status '{request.Status}' ga o'zgartirildi"
            });
        }

        [RequirePermission("admin.block")]
        [HttpPost("{id:guid}/block")]
        public async Task<IActionResult> BlockUser(Guid id)
        {
            var result = await _userService.UpdateUserStatusAsync(id, UserStatus.Blocked);
            if (!result)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Foydalanuvchi topilmadi" });

            return Ok(new ApiResponse<object> { Success = true, Message = "Foydalanuvchi bloklandi" });
        }

        [RequirePermission("admin.approve")]
        [HttpPost("{id:guid}/activate")]
        public async Task<IActionResult> ActivateUser(Guid id)
        {
            var result = await _userService.UpdateUserStatusAsync(id, UserStatus.Approved);
            if (!result)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Foydalanuvchi topilmadi" });

            return Ok(new ApiResponse<object> { Success = true, Message = "Foydalanuvchi faollashtirildi" });
        }

        private Guid? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }

        private ApiResponse<object> ValidationError() => new()
        {
            Success = false,
            Message = "Ma'lumotlar noto'g'ri",
            Errors  = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList()
        };
    }

    public class UpdateStatusRequest
    {
        public UserStatus Status { get; set; }
    }
}
