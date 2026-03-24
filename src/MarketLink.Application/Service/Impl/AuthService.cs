using MarketLink.Application.Models.Company;
using MarketLink.Application.Models.DeviceManagement;
using MarketLink.Application.Models.Login;
using MarketLink.Application.Models.Password;
using MarketLink.Application.Models.Shop;
using MarketLink.Application.Models.Token;
using MarketLink.Application.Models.User;
using MarketLink.Application.Models.UserOtp;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TokenResponse = MarketLink.Application.Models.Token.TokenResponse;
using UserRoleEntity = MarketLink.Domain.Entities.UserRole;
using RefreshTokenEntity = MarketLink.Domain.Entities.RefreshToken;
using CompanyEntity = MarketLink.Domain.Entities.Company;
using ShopEntity = MarketLink.Domain.Entities.Shop;

namespace MarketLink.Application.Service.Impl
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext  _context;
        private readonly IUserService  _userService;
        private readonly ISmsService   _smsService;
        private readonly IJwtService   _jwtService;
        private readonly IMemoryCache  _cache;

        private static readonly Guid CompanyRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly Guid ShopRoleId    = Guid.Parse("00000000-0000-0000-0000-000000000002");

        public AuthService(
            AppDbContext context,
            IUserService userService,
            ISmsService  smsService,
            IJwtService  jwtService,
            IMemoryCache cache)
        {
            _context     = context;
            _userService = userService;
            _smsService  = smsService;
            _jwtService  = jwtService;
            _cache       = cache;
        }
        public async Task<(bool Success, string Message, RegisterResponse? Data)> RegisterCompanyAsync(
            RegisterCompanyRequest request)
        {
            var existing = await _userService.GetUserByPhoneAsync(request.PhoneNumber);
            if (existing != null)
                return (false, "Bu telefon raqam allaqachon ro'yxatdan o'tgan", null);

            var user = await _userService.CreateUserAsync(request.PhoneNumber, request.Password);

            var otp = GenerateOtp();
            user.PhoneVerificationToken  = otp;
            user.PhoneVerificationExpiry = DateTime.UtcNow.AddMinutes(10);
            _cache.Set($"otp:{CleanPhone(user.PhoneNumber)}", otp, TimeSpan.FromMinutes(10));

            _context.Companies.Add(new CompanyEntity
            {
                UserId         = user.Id,
                FounderName    = request.FounderName,
                CompanyName    = request.CompanyName,
                Address        = request.Address,
                ProductionType = request.ProductionType,
                Description    = request.Description
            });

            _context.UserRoles.Add(new UserRoleEntity { UserId = user.Id, RoleId = CompanyRoleId });
            await _context.SaveChangesAsync();

            await _smsService.SendOtpSmsAsync(user.PhoneNumber, otp);

            return (true, "Ro'yxatdan o'tish muvaffaqiyatli. OTP kod yuborildi.", new RegisterResponse
            {
                UserId      = user.Id,
                PhoneNumber = user.PhoneNumber,
                Status      = user.Status,
                Message     = "OTP kod 10 daqiqa davomida amal qiladi"
            });
        }

        public async Task<(bool Success, string Message, RegisterResponse? Data)> RegisterShopAsync(
            RegisterShopRequest request)
        {
            var existing = await _userService.GetUserByPhoneAsync(request.PhoneNumber);
            if (existing != null)
                return (false, "Bu telefon raqam allaqachon ro'yxatdan o'tgan", null);

            var user = await _userService.CreateUserAsync(request.PhoneNumber, request.Password);

            var otp = GenerateOtp();
            user.PhoneVerificationToken  = otp;
            user.PhoneVerificationExpiry = DateTime.UtcNow.AddMinutes(10);
            _cache.Set($"otp:{CleanPhone(user.PhoneNumber)}", otp, TimeSpan.FromMinutes(10));

            _context.Shops.Add(new ShopEntity
            {
                UserId      = user.Id,
                FounderName = request.FounderName,
                ShopName    = request.ShopName,
                Address     = request.Address,
                ShopType    = request.ShopType,
                Description = request.Description,
                CreatedAt   = DateTime.UtcNow
            });

            _context.UserRoles.Add(new UserRoleEntity { UserId = user.Id, RoleId = ShopRoleId });
            await _context.SaveChangesAsync();

            await _smsService.SendOtpSmsAsync(user.PhoneNumber, otp);

            return (true, "Ro'yxatdan o'tish muvaffaqiyatli. OTP kod yuborildi.", new RegisterResponse
            {
                UserId      = user.Id,
                PhoneNumber = user.PhoneNumber,
                Status      = user.Status,
                Message     = "OTP kod 10 daqiqa davomida amal qiladi"
            });
        }

        public async Task<(bool Success, string Message, LoginResponse? Response)> LoginAsync(LoginRequest request)
        {
            var user = await _userService.GetUserByPhoneAsync(request.PhoneNumber);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return (false, "Telefon raqam yoki parol noto'g'ri", null);

            if (!user.IsPhoneVerified)
                return (false, "Telefon raqam tasdiqlanmagan. Iltimos, OTP kodni kiriting.", null);

            if (user.Status == UserStatus.Blocked)
                return (false, "Bu akkaunt bloklangan", null);

            var accessToken = _jwtService.GenerateAccessToken(user);

            var refreshToken = new RefreshTokenEntity
            {
                Id          = Guid.NewGuid(),
                UserId      = user.Id,
                Token       = _jwtService.GenerateRefreshToken(),
                ExpiresAt   = DateTime.UtcNow.AddDays(30),
                DeviceId    = request.DeviceId,
                Platform    = request.Platform,
                DeviceModel = request.DeviceModel,
                AppVersion  = request.AppVersion,
                PushToken   = request.PushToken,
                CreatedAt   = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            var profile = await _userService.GetUserProfileAsync(user.Id);

            return (true, "Tizimga kirish muvaffaqiyatli", new LoginResponse
            {
                AccessToken     = accessToken,
                RefreshToken    = refreshToken.Token,
                Status          = user.Status,
                IsPhoneVerified = user.IsPhoneVerified,
                Profile         = profile!
            });
        }

        public async Task<(bool Success, string Message)> VerifyPhoneNumberAsync(VerifyPhoneNumberRequest request)
        {
            var user = await _userService.GetUserByPhoneAsync(request.PhoneNumber);
            if (user == null)
                return (false, "Foydalanuvchi topilmadi");

            if (user.IsPhoneVerified)
                return (false, "Telefon raqam allaqachon tasdiqlangan");

            if (user.PhoneVerificationToken != request.OtpCode)
                return (false, "OTP kod noto'g'ri");

            if (user.PhoneVerificationExpiry < DateTime.UtcNow)
                return (false, "OTP kod muddati tugagan. Yangi kod so'rang.");

            user.IsPhoneVerified         = true;
            user.Status                  = UserStatus.Approved;
            user.PhoneVerificationToken  = null;
            user.PhoneVerificationExpiry = null;
            await _context.SaveChangesAsync();
            _cache.Remove($"otp:{CleanPhone(user.PhoneNumber)}");

            var name = user.Company?.CompanyName ?? user.Shop?.ShopName ?? "Foydalanuvchi";
            await _smsService.SendWelcomeSmsAsync(user.PhoneNumber, name);

            return (true, "Telefon raqam muvaffaqiyatli tasdiqlandi");
        }
        public async Task<(bool Success, string Message)> ResendOtpAsync(ResendOtpRequest request)
        {
            var user = await _userService.GetUserByPhoneAsync(request.PhoneNumber);
            if (user == null)
                return (false, "Foydalanuvchi topilmadi");

            if (user.IsPhoneVerified)
                return (false, "Telefon raqam allaqachon tasdiqlangan");

            if (user.PhoneVerificationExpiry.HasValue &&
                user.PhoneVerificationExpiry.Value > DateTime.UtcNow.AddMinutes(9))
                return (false, "OTP kod 1 daqiqadan keyin so'ralishi mumkin");

            var otp = GenerateOtp();
            user.PhoneVerificationToken  = otp;
            user.PhoneVerificationExpiry = DateTime.UtcNow.AddMinutes(10);
            _cache.Set($"otp:{CleanPhone(user.PhoneNumber)}", otp, TimeSpan.FromMinutes(10));
            await _context.SaveChangesAsync();

            await _smsService.SendOtpSmsAsync(user.PhoneNumber, otp);

            return (true, "Yangi OTP kod yuborildi");
        }

        public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _userService.GetUserByPhoneAsync(request.PhoneNumber);
            if (user == null)
                return (true, "Agar telefon mavjud bo'lsa, parolni tiklash kodi yuborildi");

            var resetCode = GenerateOtp();
            user.PasswordResetToken  = resetCode;
            user.PasswordResetExpiry = DateTime.UtcNow.AddMinutes(15);
            _cache.Set($"otp:{CleanPhone(user.PhoneNumber)}", resetCode, TimeSpan.FromMinutes(15));
            await _context.SaveChangesAsync();

            await _smsService.SendPasswordResetSmsAsync(user.PhoneNumber, resetCode);

            return (true, "Parolni tiklash kodi telefon raqamingizga yuborildi");
        }
        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userService.GetUserByPhoneAsync(request.PhoneNumber);
            if (user == null)
                return (false, "Foydalanuvchi topilmadi");

            if (user.PasswordResetToken != request.ResetCode)
                return (false, "Tiklash kodi noto'g'ri");

            if (user.PasswordResetExpiry < DateTime.UtcNow)
                return (false, "Tiklash kodi muddati tugagan");

            user.PasswordHash        = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken  = null;
            user.PasswordResetExpiry = null;
            await _context.SaveChangesAsync();
            _cache.Remove($"otp:{CleanPhone(user.PhoneNumber)}");

            return (true, "Parol muvaffaqiyatli o'zgartirildi");
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return (false, "Foydalanuvchi topilmadi");

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
                return (false, "Joriy parol noto'g'ri");

            if (request.NewPassword != request.ConfirmPassword)
                return (false, "Yangi parollar mos kelmaydi");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.UpdatedAt    = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, "Parol muvaffaqiyatli o'zgartirildi");
        }
        public async Task<(bool Success, string Message, TokenResponse? Response)> RefreshTokenAsync(
            RefreshTokenRequest request)
        {
            var tokenEntity = await _context.RefreshTokens
                .Include(rt => rt.User)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (tokenEntity == null)
                return (false, "Refresh token topilmadi", null);

            if (tokenEntity.IsRevoked)
                return (false, "Refresh token bekor qilingan", null);

            if (tokenEntity.ExpiresAt < DateTime.UtcNow)
                return (false, "Refresh token muddati tugagan", null);

            tokenEntity.IsRevoked = true;

            var newAccess = _jwtService.GenerateAccessToken(tokenEntity.User);
            var newRefresh = new RefreshTokenEntity
            {
                Id          = Guid.NewGuid(),
                UserId      = tokenEntity.UserId,
                Token       = _jwtService.GenerateRefreshToken(),
                ExpiresAt   = DateTime.UtcNow.AddDays(30),
                DeviceId    = tokenEntity.DeviceId,
                Platform    = tokenEntity.Platform,
                DeviceModel = tokenEntity.DeviceModel,
                CreatedAt   = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(newRefresh);
            await _context.SaveChangesAsync();

            return (true, "Token yangilandi", new TokenResponse
            {
                AccessToken  = newAccess,
                RefreshToken = newRefresh.Token
            });
        }

        public async Task LogoutAsync(Guid userId)
        {
            await RevokeAllTokensAsync(userId);
        }

        public async Task LogoutAllDevicesAsync(Guid userId)
        {
            await RevokeAllTokensAsync(userId);
        }

        public async Task<UserProfileResponse?> GetProfileAsync(Guid userId)
        {
            return await _userService.GetUserProfileAsync(userId);
        }

        public async Task<List<ActiveDeviceResponse>> GetActiveDevicesAsync(Guid userId, string? currentToken = null)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(rt => rt.LastUsedAt ?? rt.CreatedAt)
                .ToListAsync();

            return tokens.Select(t => new ActiveDeviceResponse
            {
                TokenId     = t.Id,
                DeviceModel = t.DeviceModel,
                Platform    = t.Platform,
                IpAddress   = t.IpAddress,
                LastUsedAt  = t.LastUsedAt,
                CreatedAt   = t.CreatedAt,
                IsCurrent   = t.Token == currentToken
            }).ToList();
        }

        private async Task RevokeAllTokensAsync(Guid userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var t in tokens)
                t.IsRevoked = true;

            await _context.SaveChangesAsync();
        }

        private static string GenerateOtp() => "11111";

        private static string CleanPhone(string phone) =>
            new string(phone.Where(char.IsDigit).ToArray());
    }
}
