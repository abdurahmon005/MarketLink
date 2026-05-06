using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Company;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Application.Service.Impl
{
    public class CompanyService : ICompanyService
    {
        private readonly AppDbContext _context;

        public CompanyService(AppDbContext context) => _context = context;

        public async Task<PagedResult<CompanyProfileResponse>> GetAllAsync(
            int page, int pageSize, CompanyDirection? type = null, CancellationToken ct = default)
        {
            var query = _context.Companies
                .AsNoTracking();

            if (type.HasValue)
                query = query.Where(c => c.ProductionType == type.Value);

            var total = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(c => c.AverageRating)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CompanyProfileResponse
                {
                    Id             = c.Id,
                    FounderName    = c.FounderName,
                    CompanyName    = c.CompanyName,
                    Address        = c.Address,
                    ProductionType = c.ProductionType,
                    Description    = c.Description,
                    LogoUrl        = c.LogoUrl,
                    CertificateUrl = c.CertificateUrl,
                    AverageRating  = c.AverageRating,
                    CreatedAt      = c.CreatedAt,
                    UpdatedAt      = c.UpdatedAt
                })
                .ToListAsync(ct);

            return new PagedResult<CompanyProfileResponse>
            {
                Items      = items,
                TotalCount = total,
                Page       = page,
                PageSize   = pageSize
            };
        }

        public async Task<CompanyProfileResponse?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var c = await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, ct);

            if (c == null) return null;

            return MapToResponse(c);
        }

        public async Task<CompanyProfileResponse?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            var c = await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);

            if (c == null) return null;

            return MapToResponse(c);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(
            Guid userId, UpdateCompanyProfileRequest request, CancellationToken ct = default)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);

            if (company == null)
                return (false, "Kompaniya profili topilmadi");

            if (request.FounderName    != null) company.FounderName    = request.FounderName;
            if (request.CompanyName    != null) company.CompanyName    = request.CompanyName;
            if (request.Address        != null) company.Address        = request.Address;
            if (request.ProductionType != null) company.ProductionType = request.ProductionType.Value;
            if (request.Description    != null) company.Description    = request.Description;

            company.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return (true, "Kompaniya profili yangilandi");
        }

        private static CompanyProfileResponse MapToResponse(Domain.Entities.Company c) => new()
        {
            Id             = c.Id,
            FounderName    = c.FounderName,
            CompanyName    = c.CompanyName,
            Address        = c.Address,
            ProductionType = c.ProductionType,
            Description    = c.Description,
            LogoUrl        = c.LogoUrl,
            CertificateUrl = c.CertificateUrl,
            AverageRating  = c.AverageRating,
            CreatedAt      = c.CreatedAt,
            UpdatedAt      = c.UpdatedAt
        };
    }
}
