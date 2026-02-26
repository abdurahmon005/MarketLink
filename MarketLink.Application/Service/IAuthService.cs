using MarketLink.Application.Models.Company;
using MarketLink.Application.Models.Login;
using MarketLink.Application.Models.Shop;
using MarketLink.Application.Models.Token;
using MarketLink.Application.Models.User;
using MarketLink.Application.Models.UserOtp;
using MarketLink.Domain.Entities;
using Microsoft.AspNetCore.Identity.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ForgotPasswordRequest = MarketLink.Application.Models.UserOtp.ForgotPasswordRequest;
using LoginRequest = MarketLink.Application.Models.Login.LoginRequest;
using ResetPasswordRequest = MarketLink.Application.Models.UserOtp.ResetPasswordRequest;
using TokenResponse = MarketLink.Application.Models.Login.TokenResponse;

namespace MarketLink.Application.Service
{
    public interface IAuthService
    {
        Task<(bool Success, string Message)> RegisterCompanyAsync(RegisterCompanyRequest request);
        Task<(bool Success, string Message)> RegisterShopAsync(RegisterShopRequest request);
        Task<(bool Success, string Message, LoginResponse? Response)> LoginAsync(LoginRequest request);
        Task<(bool Success, string Message)> VerifyEmailAsync(VerifyEmailRequest request);
        Task<(bool Success, string Message)> ResendOtpAsync(ResendOtpRequest request);
        Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request);
        Task<(bool Success, string Message, TokenResponse? Response)> RefreshTokenAsync(RefreshTokenRequest request);
        Task<UserProfileResponse?> GetUserProfileAsync(Guid userId);
        Task LogoutAsync(Guid userId);
    }

}
