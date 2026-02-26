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
        private readonly IUserService _userService;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;

        private static readonly Guid CompanyRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        private static readonly Guid ShopRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        public AuthService(AppDbContext context, IUserService userService, IOtpService otpService, IEmailService emailService, IJwtService jwtService)
        {
            _context = context;
            _userService = userService;
            _otpService = otpService;
            _emailService = emailService;
            _jwtService = jwtService;
        }

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

        public Task LogoutAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<(bool Success, string Message, Models.Login.TokenResponse? Response)> RefreshTokenAsync(RefreshTokenRequest request)
        {
            throw new NotImplementedException();
        }

        //public async Task<(bool Success, string Message)> RegisterCompanyAsync(RegisterCompanyRequest request)
        //{
        //    if (await _userService.GetUserByEmailAsync(request.Email) != null)
        //        return (false, "Bu email allaqachon ro'yxatdan o'tgan", null);

             
        //}

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

        public Task<(bool Success, string Message)> RegisterCompanyAsync(RegisterCompanyRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
