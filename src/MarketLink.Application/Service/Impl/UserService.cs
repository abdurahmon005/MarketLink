using MarketLink.Application.Models.Company;
using MarketLink.Application.Models.Shop;
using MarketLink.Application.Models.User;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Entities;
using MarketLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Application.Service.Impl
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context) => _context = context;

        private static string NormalizePhone(string phone) =>
            new string(phone.Where(char.IsDigit).ToArray());

        public async Task<User> CreateUserAsync(string phoneNumber, string password)
        {
            var user = new User
            {
                Id           = Guid.NewGuid(),
                PhoneNumber  = NormalizePhone(phoneNumber),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Status       = UserStatus.Pending,
                CreatedAt    = DateTime.UtcNow,
                UpdatedAt    = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }
        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.Company)
                .Include(u => u.Shop)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
        public async Task<User?> GetUserByPhoneAsync(string phoneNumber)
        {
            var clean = NormalizePhone(phoneNumber);
            return await _context.Users
                .Include(u => u.Company)
                .Include(u => u.Shop)
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.PhoneNumber == clean);
        }

        public async Task<UserProfileResponse?> GetUserProfileAsync(Guid userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null) return null;

            return new UserProfileResponse
            {
                Id              = user.Id,
                PhoneNumber     = user.PhoneNumber,
                Status          = user.Status,
                IsPhoneVerified = user.IsPhoneVerified,
                Roles           = user.UserRoles?.Select(ur => ur.Role.Name).ToList() ?? new(),
                CreatedAt       = user.CreatedAt,
                Company = user.Company != null ? new CompanyProfileResponse
                {
                    Id             = user.Company.Id,
                    FounderName    = user.Company.FounderName,
                    CompanyName    = user.Company.CompanyName,
                    Address        = user.Company.Address,
                    ProductionType = user.Company.ProductionType,
                    Description    = user.Company.Description,
                    AvarageRaiting = user.Company.AvarageRaiting,
                    LogoUrl        = user.Company.LogoUrl,
                    SertificateUrl = user.Company.SertificateUrl,
                    CreatedAt      = user.CreatedAt
                } : null,
                Shop = user.Shop != null ? new ShopProfileResponse
                {
                    Id          = user.Shop.Id,
                    FounderName = user.Shop.FounderName,
                    ShopName    = user.Shop.ShopName,
                    Address     = user.Shop.Address,
                    ShopType    = user.Shop.ShopType,
                    Description = user.Shop.Description,
                    SertificateUrl = user.Shop.SertificateUrl,
                    CreatedAt   = user.Shop.CreatedAt
                } : null
            };
        }

        public async Task<(bool Success, string Message)> UpdateCompanyProfileAsync(
            Guid userId, UpdateCompanyProfileRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Company == null)
                return (false, "Company profili topilmadi");

            if (request.FounderName    != null) user.Company.FounderName    = request.FounderName;
            if (request.CompanyName    != null) user.Company.CompanyName    = request.CompanyName;
            if (request.Address        != null) user.Company.Address        = request.Address;
            if (request.ProductionType != null) user.Company.ProductionType = request.ProductionType.Value;
            if (request.Description    != null) user.Company.Description    = request.Description;

            await _context.SaveChangesAsync();

            return (true, "Company profili yangilandi");
        }

        public async Task<(bool Success, string Message)> UpdateShopProfileAsync(
            Guid userId, UpdateShopProfileRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Shop)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.Shop == null)
                return (false, "Do'kon profili topilmadi");

            if (request.FounderName != null) user.Shop.FounderName = request.FounderName;
            if (request.ShopName    != null) user.Shop.ShopName    = request.ShopName;
            if (request.Address     != null) user.Shop.Address     = request.Address;
            if (request.ShopType    != null) user.Shop.ShopType    = request.ShopType.Value;
            if (request.Description != null) user.Shop.Description = request.Description;

            user.Shop.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, "Do'kon profili yangilandi");
        }

        public async Task<bool> UpdateUserStatusAsync(Guid userId, UserStatus status)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.Status    = status;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdatePasswordAsync(Guid userId, string newPasswordHash)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.PasswordHash = newPasswordHash;
            user.UpdatedAt    = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ServiceResult> VerifyPhoneAsync(Guid userId, CancellationToken ct = default)
        {
            var user = await _context.Users.FindAsync(new object[] { userId }, ct);
            if (user == null) return ServiceResult.Fail("Foydalanuvchi topilmadi");

            user.IsPhoneVerified = true;
            user.Status          = UserStatus.Approved;
            user.UpdatedAt       = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return ServiceResult.Ok("Telefon tasdiqlandi");
        }
    }
}
