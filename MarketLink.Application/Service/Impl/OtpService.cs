//using MarketLink.DataAccess.Persistence;
//using MarketLink.Domain.Entities;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;

//namespace MarketLink.Application.Service.Impl
//{
//    public class OtpService : IOtpService
//    {
//        private readonly AppDbContext _context;
//        private readonly IPhoneNumberService _phoneNumberService;
//        private readonly ISmsService _smsService;
//        private readonly ILogger<OtpService> _logger;
//        private const int OTP_LENGTH = 6;
//        private const int OTP_EXPIRY_MINUTES = 5;
//        private const int MAX_ATTEMPTS = 3;
//        public OtpService(AppDbContext context, IPhoneNumberService phoneNumberService, ISmsService smsService)
//        {
//            _context = context;
//            _phoneNumberService = phoneNumberService;
//            _smsService = smsService;
//        }

//        public async Task<string> GenerateAndSaveOtpAsync(Guid userId)
//        {
//            var user = await _context.Users.FindAsync(userId);
//            if (user == null)
//                throw new Exception("User not found");

//            var otpCode = new Random().Next(100000, 999999).ToString();

//            var otp = new UserOTPs
//            {
//                UserId = userId,
//                Code = otpCode,
//                CreatedAt = DateTime.UtcNow,
//                ExpiredAt = DateTime.UtcNow.AddMinutes(5)
//            };

//            await _context.UserOTPs.AddAsync(otp);
//            await _context.SaveChangesAsync();

//            return otpCode;
//        }

//        public async Task<UserOTPs?> GetLatestOtpAsync(Guid userId, string code)
//        {
//            return await _context.UserOTPs
//                .Where(o => o.UserId == userId && o.Code == code && o.ExpiredAt > DateTime.UtcNow)
//                .OrderByDescending(o => o.CreatedAt)
//                .FirstOrDefaultAsync();
//        }

//        public async Task<bool> SentOtpAsync(string phoneNumber)
//        {
//            try
//            {
//                phoneNumber = FormatPhoneNumber(phoneNumber);

//                var recentOtp = await _context.OtpCodes
//               .Where(o => o.PhoneNumber == phoneNumber &&
//                          o.CreatedAt > DateTime.UtcNow.AddMinutes(-1))
//               .FirstOrDefaultAsync();


//                if (recentOtp != null)
//                {
//                    _logger.LogWarning($"OTP allaqachon yuborilgan: {phoneNumber}");
//                    return false;
//                }

//                //eski otpni bekor qilish uchun bu
//                var oldCodes = await _context.OtpCodes
//               .Where(o => o.PhoneNumber == phoneNumber && !o.IsUsed)
//               .ToListAsync();

//                foreach (var code in oldCodes)
//                {
//                    code.IsUsed = true;
//                }
//                var otpCode = await GenerateAndSaveOtpAsync();

//                // SMS yuborish
//                string message = $"Tasdiqlash kodi: {otpCode}\n{OTP_EXPIRY_MINUTES} daqiqa amal qiladi.";
//                bool smsSent = await _smsService.SendSmsAsync(phoneNumber, message);

//                if (!smsSent)
//                {
//                    _logger.LogError($"SMS yuborishda xatolik: {phoneNumber}");
//                    throw new Exception("SMS yuborishda xatolik");
//                }

//                _logger.LogInformation($"OTP yuborildi: {phoneNumber}");
//                return otpCode;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"OTP yuborishda xatolik: {phoneNumber}");
//                throw;
//            }
//        }

//        public Task<(bool isValid, User user)> VerifyOtpAsync(string phoneNumber, string code)
//        {
//            throw new NotImplementedException();
//        }
//        private string FormatPhoneNumber(string phoneNumber)
//        {
//            // Telefon raqamni formatlash (faqat raqamlar qoldirish)
//            return new string(phoneNumber.Where(char.IsDigit).ToArray());
//        }
//    }
//}
