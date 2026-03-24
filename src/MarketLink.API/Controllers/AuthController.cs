using MarketLink.API.Common;
using MarketLink.Application.Models.Company;
using MarketLink.Application.Models.Login;
using MarketLink.Application.Models.Password;
using MarketLink.Application.Models.Shop;
using MarketLink.Application.Models.Token;
using MarketLink.Application.Models.UserOtp;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService            _authService;
        private readonly IOtpService             _otpService;
        private readonly IWebHostEnvironment     _env;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IOtpService otpService,
            IWebHostEnvironment env, ILogger<AuthController> logger)
        {
            _authService = authService;
            _otpService  = otpService;
            _env         = env;
            _logger      = logger;
        }

        [HttpPost("register/company")]
        public async Task<IActionResult> RegisterCompany([FromBody] RegisterCompanyRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ValidationError());

            var (success, message, data) = await _authService.RegisterCompanyAsync(request);

            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message, Data = data });
        }

        [HttpPost("register/shop")]
        public async Task<IActionResult> RegisterShop([FromBody] RegisterShopRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ValidationError());

            var (success, message, data) = await _authService.RegisterShopAsync(request);

            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message, Data = data });
        }

        // ──────────────── LOGIN ────────────────

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ValidationError());

            var (success, message, response) = await _authService.LoginAsync(request);

            if (!success)
                return Unauthorized(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message, Data = response });
        }


        [HttpPost("verify-phone")]
        public async Task<IActionResult> VerifyPhone([FromBody] VerifyPhoneNumberRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ValidationError());

            var (success, message) = await _authService.VerifyPhoneNumberAsync(request);

            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ValidationError());

            var (success, message) = await _authService.ResendOtpAsync(request);

            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ValidationError());

            var (_, message) = await _authService.ForgotPasswordAsync(request);

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ValidationError());

            var (success, message) = await _authService.ResetPasswordAsync(request);

            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ValidationError());

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Token noto'g'ri" });

            var (success, message) = await _authService.ChangePasswordAsync(userId.Value, request);

            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }


        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ValidationError());

            var (success, message, response) = await _authService.RefreshTokenAsync(request);

            if (!success)
                return Unauthorized(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message, Data = response });
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            await _authService.LogoutAsync(userId.Value);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Tizimdan muvaffaqiyatli chiqildi"
            });
        }

        [Authorize]
        [HttpPost("logout-all")]
        public async Task<IActionResult> LogoutAll()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            await _authService.LogoutAllDevicesAsync(userId.Value);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Barcha qurilmalardan muvaffaqiyatli chiqildi"
            });
        }


        [Authorize]
        [HttpGet("devices")]
        public async Task<IActionResult> GetActiveDevices()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var currentToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            var devices = await _authService.GetActiveDevicesAsync(userId.Value, currentToken);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Faol qurilmalar",
                Data    = devices
            });
        }


        /// <summary>
        /// FAQAT DEVELOPMENT: telefon raqami bo'yicha OTPni qaytaradi (Redis cache dan)
        /// </summary>
        [HttpGet("dev/otp")]
        public async Task<IActionResult> GetDevOtp([FromQuery] string phone)
        {
            if (!_env.IsDevelopment())
                return NotFound();

            if (string.IsNullOrWhiteSpace(phone))
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Telefon raqami kiritilmagan" });

            var otp = await _otpService.GetOtpFromCacheAsync(phone);

            if (otp == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "OTP topilmadi yoki muddati tugagan" });

            return Ok(new ApiResponse<object> { Success = true, Data = new { phone, otp } });
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
}
