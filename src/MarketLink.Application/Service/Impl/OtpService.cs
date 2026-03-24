using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MarketLink.Application.Service.Impl
{
    public class OtpService : IOtpService
    {
        private readonly AppDbContext          _context;
        private readonly ISmsService           _smsService;
        private readonly ILogger<OtpService>   _logger;
        private readonly IMemoryCache          _cache;
        private const    int OtpExpiryMinutes  = 5;

        public OtpService(AppDbContext context, ISmsService smsService,
            ILogger<OtpService> logger, IMemoryCache cache)
        {
            _context    = context;
            _smsService = smsService;
            _logger     = logger;
            _cache      = cache;
        }

        public async Task<string> GenerateAndSaveOtpAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) throw new Exception("Foydalanuvchi topilmadi");

            var code = new Random().Next(100000, 999999).ToString();

            var otp = new UserOTPs
            {
                UserId    = userId,
                Code      = code,
                CreatedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes)
            };

            await _context.UserOTPs.AddAsync(otp);
            await _context.SaveChangesAsync();

            return code;
        }

        public async Task<UserOTPs?> GetLatestOtpAsync(Guid userId, string code)
        {
            return await _context.UserOTPs
                .Where(o => o.UserId == userId && o.Code == code && o.ExpiredAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> SentOtpAsync(string phoneNumber)
        {
            try
            {
                var cleanPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());

                // 1 daqiqa ichida qayta yuborish cheklovi
                var recentOtp = await _context.OtpCodes
                    .Where(o => o.PhoneNumber == cleanPhone &&
                                o.CreatedAt > DateTime.UtcNow.AddMinutes(-1))
                    .FirstOrDefaultAsync();

                if (recentOtp != null)
                {
                    _logger.LogWarning("OTP allaqachon yuborilgan: {Phone}", cleanPhone);
                    return false;
                }

                var oldCodes = await _context.OtpCodes
                    .Where(o => o.PhoneNumber == cleanPhone && !o.IsUsed)
                    .ToListAsync();

                foreach (var old in oldCodes)
                    old.IsUsed = true;

                var otpCode = new Random().Next(100000, 999999).ToString();

                _context.OtpCodes.Add(new OtpCode
                {
                    PhoneNumber = cleanPhone,
                    Code        = otpCode,
                    CreatedAt   = DateTime.UtcNow,
                    ExpiresAt   = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes)
                });

                await _context.SaveChangesAsync();

                // Memory cache ga saqlash (dev uchun GetOtpFromCacheAsync ishlatadi)
                _cache.Set($"otp:{cleanPhone}", otpCode, TimeSpan.FromMinutes(OtpExpiryMinutes));

                var message = $"Tasdiqlash kodi: {otpCode}\n{OtpExpiryMinutes} daqiqa amal qiladi.";
                var sent    = await _smsService.SendSmsAsync(cleanPhone, message);

                if (!sent)
                    _logger.LogError("SMS yuborishda xatolik: {Phone}", cleanPhone);

                return sent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OTP yuborishda xatolik: {Phone}", phoneNumber);
                throw;
            }
        }

        public Task<string?> GetOtpFromCacheAsync(string phoneNumber)
        {
            var cleanPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());
            _cache.TryGetValue($"otp:{cleanPhone}", out string? otp);
            return Task.FromResult(otp);
        }

        public async Task<(bool isValid, User user)> VerifyOtpAsync(string phoneNumber, string code)
        {
            var cleanPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());

            var otpCode = await _context.OtpCodes
                .Where(o => o.PhoneNumber == cleanPhone && o.Code == code &&
                            !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpCode == null)
                return (false, null!);

            otpCode.IsUsed = true;
            await _context.SaveChangesAsync();

            _cache.Remove($"otp:{cleanPhone}");

            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.PhoneNumber == cleanPhone);

            return user != null ? (true, user) : (false, null!);
        }
    }
}
