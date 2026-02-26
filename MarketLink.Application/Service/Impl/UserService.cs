using MarketLink.Application.Models.Company;
using MarketLink.Application.Models.Shop;
using MarketLink.Application.Models.User;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Entities;
using MarketLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Service.Impl
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context) => _context = context;

        public Task ClearRefreshTokenAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ConfirmEmailAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task<User> CreateUserAsync(string email, string password)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = PasswordHasher.HashPassword(password),
                Status = UserStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Company)
                .Include(u => u.Shop)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users
              .Include(u => u.Company)
              .Include(u => u.Shop)
              .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
              .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<UserProfileResponse?> GetUserProfileAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role)
                .Include(x => x.Company)
                .Include(x => x.Shop)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                return null;

            return new UserProfileResponse
            {
                Id = user.Id,
                Email = user.Email,
                Status = user.Status,

                Roles = user.UserRoles
                    .Select(ur => ur.Role.Name)
                    .ToList(),

                Company = user.Company == null ? null : new CompanyProfileResponse
                {
                    Id = user.Company.Id,
                    FounderName = user.Company.FounderName,
                    CompanyName = user.Company.CompanyName,
                    Address = user.Company.Address,
                    ProductionType = user.Company.ProductionType,
                    Description = user.Company.Description,
                    LogoUrl = user.Company.LogoUrl,
                    SertificateUrl = user.Company.SertificateUrl,
                    AvarageRaiting = user.Company.AvarageRaiting
                },

                Shop = user.Shop == null ? null : new ShopProfileResponse
                {
                    Id = user.Shop.Id,
                    FounderName = user.Shop.FounderName,
                    ShopName = user.Shop.ShopName,
                    Address = user.Shop.Address,
                    ShopType = user.Shop.ShopType,
                    Description = user.Shop.Description,
                    LogoUrl = user.Shop.LogoUrl,
                    SertificateUrl = user.Shop.SertificateUrl,
                    CreatedAt = user.Shop.CreatedAt,
                    UpdatedAt = user.Shop.UpdatedAt
                }
            };
        }

        public Task SaveRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiry)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdatePasswordAsync(Guid userId, string newPasswordHash)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.PasswordHash = newPasswordHash;
            user.PasswordResetToken = null;
            user.PasswordResetExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public Task<bool> UpdateUserStatusAsync(Guid userId, UserStatus status)
        {
            throw new NotImplementedException();
        }
    }
}
