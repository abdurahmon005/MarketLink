using MarketLink.Application.Models.Shop;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketLink.Application.Service.Impl
{
    public class ShopProfileService : IShopProfileService
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<ShopProfileService> _logger;

        public ShopProfileService(
            AppDbContext context,
            IFileService fileService,
            ILogger<ShopProfileService> logger)
        {
            _context     = context;
            _fileService = fileService;
            _logger      = logger;
        }

        public async Task<ShopProfileResponse?> GetProfileAsync(
            Guid userId, CancellationToken ct = default)
        {
            var shop = await _context.Shops
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId, ct);

            if (shop == null) return null;

            return new ShopProfileResponse
            {
                Id             = shop.Id,
                FounderName    = shop.FounderName,
                ShopName       = shop.ShopName,
                Address        = shop.Address,
                ShopType       = shop.ShopType,
                Description    = shop.Description,
                LogoUrl        = shop.LogoUrl,
                CertificateUrl = shop.CertificateUrl,
                CreatedAt      = shop.CreatedAt,
                UpdatedAt      = shop.UpdatedAt
            };
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(
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

        public async Task<(bool Success, string Message)> UpdateMediaAsync(
            Guid userId, IFormFile? logo, IFormFile? certificate, CancellationToken ct = default)
        {
            var shop = await _context.Shops
                .FirstOrDefaultAsync(s => s.UserId == userId, ct);

            if (shop == null)
                return (false, "Do'kon profili topilmadi");

            if (logo != null)
            {
                try
                {
                    var upload = await _fileService.UploadAsync(logo, FileType.Logo, Guid.NewGuid(), ct);
                    shop.LogoUrl = upload.Url ?? upload.ObjectPath;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Logo yuklashda xato. ShopId: {ShopId}", shop.Id);
                    return (false, "Logo yuklashda xato yuz berdi");
                }
            }

            if (certificate != null)
            {
                try
                {
                    var upload = await _fileService.UploadAsync(
                        certificate, FileType.Certificate, Guid.NewGuid(), ct);
                    shop.CertificateUrl = upload.ObjectPath;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Guvohnoma yuklashda xato. ShopId: {ShopId}", shop.Id);
                    return (false, "Guvohnoma yuklashda xato yuz berdi");
                }
            }

            shop.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return (true, "Media fayllar yangilandi");
        }
    }
}
