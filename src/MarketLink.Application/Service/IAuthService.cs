using MarketLink.Application.Models.Company;
using MarketLink.Application.Models.DeviceManagement;
using MarketLink.Application.Models.Login;
using MarketLink.Application.Models.Password;
using MarketLink.Application.Models.Shop;
using MarketLink.Application.Models.Token;
using MarketLink.Application.Models.User;
using MarketLink.Application.Models.UserOtp;
using TokenResponse = MarketLink.Application.Models.Token.TokenResponse;

namespace MarketLink.Application.Service
{
    public interface IAuthService
    {
        // Registratsiya
        Task<(bool Success, string Message, RegisterResponse? Data)> RegisterCompanyAsync(RegisterCompanyRequest request);
        Task<(bool Success, string Message, RegisterResponse? Data)> RegisterShopAsync(RegisterShopRequest request);

        // Login
        Task<(bool Success, string Message, LoginResponse? Response)> LoginAsync(LoginRequest request);

        // Telefon tasdiqlash
        Task<(bool Success, string Message)> VerifyPhoneNumberAsync(VerifyPhoneNumberRequest request);
        Task<(bool Success, string Message)> ResendOtpAsync(ResendOtpRequest request);

        // Parol
        Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request);
        Task<(bool Success, string Message)> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);

        // Token
        Task<(bool Success, string Message, TokenResponse? Response)> RefreshTokenAsync(RefreshTokenRequest request);

        // Logout
        Task LogoutAsync(Guid userId);
        Task LogoutAllDevicesAsync(Guid userId);

        // Profil
        Task<UserProfileResponse?> GetProfileAsync(Guid userId);
        Task<List<ActiveDeviceResponse>> GetActiveDevicesAsync(Guid userId, string? currentToken = null);
    }
}
