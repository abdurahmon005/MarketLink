using MarketLink.Application.Models.User;
using MarketLink.Application.Models.UserOtp;
using MarketLink.Domain.Entities;
using MarketLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Service
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(string email, string password);

        Task<User?> GetUserByEmailAsync(string email);

        Task<User?> GetUserByIdAsync(Guid userId);

        Task<UserProfileResponse?> GetUserProfileAsync(Guid userId);

        Task<bool> ConfirmEmailAsync(Guid userId);

        Task<bool> UpdatePasswordAsync(Guid userId, string newPasswordHash);

        Task<bool> UpdateUserStatusAsync(Guid userId, UserStatus status);

        Task SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiry);

        Task ClearRefreshTokenAsync(Guid userId);
    }

}

