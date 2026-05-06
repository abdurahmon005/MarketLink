using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Shop;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Application.Service.Impl
{
    public class ShopService : IShopService
    {
        private readonly AppDbContext _context;

        public ShopService(AppDbContext context) => _context = context;

        public async Task<PagedResult<ShopProfileResponse>> GetAllAsync(
            int page, int pageSize, ShopType? type = null, CancellationToken ct = default)
        {
            var query = _context.Shops.AsNoTracking();

            if (type.HasValue)
                query = query.Where(s => s.ShopType == type.Value);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ShopProfileResponse
                {
                    Id             = s.Id,
                    FounderName    = s.FounderName,
                    ShopName       = s.ShopName,
                    Address        = s.Address,
                    ShopType       = s.ShopType,
                    Description    = s.Description,
                    LogoUrl        = s.LogoUrl,
                    CertificateUrl = s.CertificateUrl,
                    CreatedAt      = s.CreatedAt,
                    UpdatedAt      = s.UpdatedAt
                })
                .ToListAsync(ct);

            return new PagedResult<ShopProfileResponse>
            {
                Items      = items,
                TotalCount = total,
                Page       = page,
                PageSize   = pageSize
            };
        }

        public async Task<ShopProfileResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var s = await _context.Shops
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            return s == null ? null : MapToResponse(s);
        }

        public async Task<ShopProfileResponse?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            var s = await _context.Shops
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId, ct);

            return s == null ? null : MapToResponse(s);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(
            Guid userId, UpdateShopProfileRequest request, CancellationToken ct = default)
        {
            var shop = await _context.Shops
                .FirstOrDefaultAsync(s => s.UserId == userId, ct);

            if (shop == null)
                return (false, "Do'kon profili topilmadi");

            if (request.FounderName != null) shop.FounderName = request.FounderName;
            if (request.ShopName    != null) shop.ShopName    = request.ShopName;
            if (request.Address     != null) shop.Address     = request.Address;
            if (request.ShopType    != null) shop.ShopType    = request.ShopType.Value;
            if (request.Description != null) shop.Description = request.Description;

            shop.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return (true, "Do'kon profili yangilandi");
        }

        private static ShopProfileResponse MapToResponse(Domain.Entities.Shop s) => new()
        {
            Id             = s.Id,
            FounderName    = s.FounderName,
            ShopName       = s.ShopName,
            Address        = s.Address,
            ShopType       = s.ShopType,
            Description    = s.Description,
            LogoUrl        = s.LogoUrl,
            CertificateUrl = s.CertificateUrl,
            CreatedAt      = s.CreatedAt,
            UpdatedAt      = s.UpdatedAt
        };
    }
}
