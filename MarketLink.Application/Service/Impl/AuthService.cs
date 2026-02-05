using MarketLink.Application.Helpers.GenerateJWT;
using MarketLink.Application.Models.Company;
using MarketLink.Application.Models.Login;
using MarketLink.Application.Models.Shop;
using MarketLink.Application.Models.Token;
using MarketLink.Application.Models.User;
using MarketLink.Application.Models.UserOtp;
using MarketLink.DataAccess.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Service.Impl
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UserProfileResponse?> GetUserProfileAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message, LoginResponse? Response)> LoginAsync(LoginRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message, Models.Login.TokenResponse? Response)> RefreshTokenAsync(RefreshTokenRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message)> RegisterCompanyAsync(RegisterCompanyRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message)> RegisterShopAsync(RegisterShopRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message)> ResendOtpAsync(ResendOtpRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message)> VerifyEmailAsync(VerifyEmailRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
