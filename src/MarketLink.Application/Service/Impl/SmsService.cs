using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace MarketLink.Application.Service.Impl
{
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;
        private readonly HttpClient          _httpClient;
        private readonly string              _email;
        private readonly string              _password;

        // Static — token barcha instancelar o'rtasida saqlanadi
        private static string?  _token;
        private static DateTime _tokenExpiry = DateTime.MinValue;
        private static readonly SemaphoreSlim _authLock = new(1, 1);

        public SmsService(IConfiguration config, ILogger<SmsService> logger, HttpClient httpClient)
        {
            _logger     = logger;
            _httpClient = httpClient;
            _email      = config["Eskiz:Email"]    ?? string.Empty;
            _password   = config["Eskiz:Password"] ?? string.Empty;
        }

        public async Task<bool> SendOtpSmsAsync(string phoneNumber, string otpCode)
        {
            var message = $"Market Link: Tasdiqlash kodi: {otpCode}. Bu kod 10 daqiqa amal qiladi.";
            return await SendSmsInternalAsync(phoneNumber, message);
        }

        public async Task<bool> SendPasswordResetSmsAsync(string phoneNumber, string resetCode)
        {
            var message = $"Market Link: Parolni tiklash kodi: {resetCode}. Bu kod 15 daqiqa amal qiladi.";
            return await SendSmsInternalAsync(phoneNumber, message);
        }

        public async Task<bool> SendWelcomeSmsAsync(string phoneNumber, string userName)
        {
            var message = $"Xush kelibsiz {userName}! Market Link platformasiga muvaffaqiyatli ro'yxatdan o'tdingiz.";
            return await SendSmsInternalAsync(phoneNumber, message);
        }

        public async Task<bool> SendSmsAsync(string phoneNumber, string message)
        {
            return await SendSmsInternalAsync(phoneNumber, message);
        }

        private async Task<bool> SendSmsInternalAsync(string phoneNumber, string message)
        {
            if (string.IsNullOrEmpty(_email) || string.IsNullOrEmpty(_password))
            {
                _logger.LogWarning("Eskiz credentials sozlanmagan. SMS yuborilmadi: {Phone}", phoneNumber);
                return true;
            }

            try
            {
                if (string.IsNullOrEmpty(_token) || _tokenExpiry <= DateTime.UtcNow)
                {
                    if (!await AuthenticateAsync())
                    {
                        _logger.LogError("Eskiz.uz: token olishda xatolik");
                        return false;
                    }
                }

                var cleanPhone = phoneNumber.Replace("+", "").Trim();

                var form = new MultipartFormDataContent
                {
                    { new StringContent(cleanPhone),     "mobile_phone" },
                    { new StringContent(message),        "message"      },
                    { new StringContent("4546"),         "from"         }
                };

                var req = new HttpRequestMessage(HttpMethod.Post,
                    "https://notify.eskiz.uz/api/message/sms/send")
                {
                    Content = form
                };
                req.Headers.Add("Authorization", $"Bearer {_token}");

                var response = await _httpClient.SendAsync(req);
                var body     = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("SMS yuborildi: {Phone}", phoneNumber);
                    return true;
                }

                _logger.LogError("SMS xatolik {Status}: {Body}", (int)response.StatusCode, body);

                // Token eskirgan bo'lsa — qayta auth qilib bir marta retry
                if ((int)response.StatusCode == 401)
                {
                    _token = null;
                    if (await AuthenticateAsync())
                        return await SendSmsInternalAsync(phoneNumber, message);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS yuborishda xatolik");
                return false;
            }
        }

        private async Task<bool> AuthenticateAsync()
        {
            await _authLock.WaitAsync();
            try
            {
                // Yana tekshiramiz — boshqa thread allaqachon yangilagan bo'lishi mumkin
                if (!string.IsNullOrEmpty(_token) && _tokenExpiry > DateTime.UtcNow)
                    return true;

                var form = new MultipartFormDataContent
                {
                    { new StringContent(_email),    "email"    },
                    { new StringContent(_password), "password" }
                };

                var response = await _httpClient.PostAsync(
                    "https://notify.eskiz.uz/api/auth/login", form);

                var body = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Eskiz auth response {Status}: {Body}",
                    (int)response.StatusCode, body);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Eskiz auth failed {Status}: {Body}", (int)response.StatusCode, body);
                    return false;
                }

                var doc = JsonDocument.Parse(body);

                if (doc.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("token", out var tokenProp))
                {
                    _token       = tokenProp.GetString();
                    _tokenExpiry = DateTime.UtcNow.AddDays(28);
                    _logger.LogInformation("Eskiz token olindi, amal qilish muddati: {Expiry}", _tokenExpiry);
                    return true;
                }

                _logger.LogError("Eskiz: token response dan olinmadi: {Body}", body);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eskiz auth xatoligi");
                return false;
            }
            finally
            {
                _authLock.Release();
            }
        }
    }
}
