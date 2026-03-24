using MarketLink.Application.Models.Company;
using MarketLink.Application.Models.Shop;
using MarketLink.Application.Models.User;
using MarketLink.Application.Service.Impl;
using MarketLink.Domain.Entities;
using MarketLink.Domain.Enums;

namespace MarketLink.Application.Service
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(string phoneNumber, string password);
        Task<User?> GetUserByPhoneAsync(string phoneNumber);
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<UserProfileResponse?> GetUserProfileAsync(Guid userId);
        Task<(bool Success, string Message)> UpdateCompanyProfileAsync(Guid userId, UpdateCompanyProfileRequest request);
        Task<(bool Success, string Message)> UpdateShopProfileAsync(Guid userId, UpdateShopProfileRequest request);
        Task<bool> UpdateUserStatusAsync(Guid userId, UserStatus status);
        Task<bool> UpdatePasswordAsync(Guid userId, string newPasswordHash);
        Task<ServiceResult> VerifyPhoneAsync(Guid userId, CancellationToken ct = default);
    }
}

