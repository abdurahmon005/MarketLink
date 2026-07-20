using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Company;
using MarketLink.Application.Models.Supplier;
using MarketLink.DataAccess.Persistence;
using MarketLink.Domain.Entities;
using MarketLink.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace MarketLink.Application.Service.Impl
{
    public class CompanyService : ICompanyService
    {
        private readonly AppDbContext _context;
        private readonly IFileService _fileService;

        public CompanyService(AppDbContext context, IFileService fileService)
        {
            _context     = context;
            _fileService = fileService;
        }

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

        // ── Supplier Panel Methods ─────────────────────────────────────────────

        public async Task<CompanyProfileDto?> GetProfileAsync(int companyId, CancellationToken ct = default)
        {
            var c = await _context.Companies
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == companyId, ct);

            if (c == null) return null;

            var totalOrders = await _context.Orders.CountAsync(o => o.CompanyId == companyId, ct);
            var activeShops = await _context.Orders
                .Where(o => o.CompanyId == companyId)
                .Select(o => o.ShopId).Distinct().CountAsync(ct);

            var phone = await _context.Users
                .Where(u => u.Id == c.UserId)
                .Select(u => u.PhoneNumber)
                .FirstOrDefaultAsync(ct);

            return new CompanyProfileDto
            {
                Id          = c.Id,
                CompanyName = c.CompanyName,
                FounderName = c.FounderName,
                Address     = c.Address,
                Phone       = phone,
                AccountType = c.ProductionType,
                Description = c.Description,
                LogoUrl     = c.LogoUrl,
                Stats       = new CompanyStatsDto
                {
                    TotalOrders = totalOrders,
                    AvgRating   = Math.Round(c.AverageRating, 2),
                    ActiveShops = activeShops
                }
            };
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(
            int companyId, UpdateCompanyRequest request, CancellationToken ct = default)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyId, ct);

            if (company == null)
                return (false, "Kompaniya topilmadi");

            if (request.CompanyName != null) company.CompanyName = request.CompanyName;
            if (request.FounderName != null) company.FounderName = request.FounderName;
            if (request.Address     != null) company.Address     = request.Address;
            if (request.Description != null) company.Description = request.Description;

            company.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            return (true, "Kompaniya profili yangilandi");
        }

        public async Task<(bool Success, string Message, string? LogoUrl)> UpdateLogoAsync(
            int companyId, IFormFile file, CancellationToken ct = default)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyId, ct);

            if (company == null)
                return (false, "Kompaniya topilmadi", null);

            try
            {
                var upload = await _fileService.UploadAsync(file, FileType.Logo, company.UserId, ct);
                company.LogoUrl   = upload.Url ?? upload.ObjectPath;
                company.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
                return (true, "Logo yangilandi", company.LogoUrl);
            }
            catch (Exception)
            {
                return (false, "Logo yuklashda xato yuz berdi", null);
            }
        }

        public async Task<List<CompanyBranchDto>> GetBranchesAsync(
            int companyId, CancellationToken ct = default)
        {
            return await _context.CompanyBranches
                .AsNoTracking()
                .Where(b => b.CompanyId == companyId)
                .Select(b => new CompanyBranchDto
                {
                    Id           = b.Id,
                    Name         = b.Name,
                    City         = b.City,
                    Address      = b.Address,
                    Phone        = b.Phone,
                    ManagerName  = b.ManagerName,
                    IsActive     = b.IsActive,
                    ActiveOrdersCount = 0
                })
                .OrderBy(b => b.Name)
                .ToListAsync(ct);
        }

        public async Task<int> AddBranchAsync(
            int companyId, CreateBranchRequest request, CancellationToken ct = default)
        {
            var branch = new CompanyBranch
            {
                CompanyId   = companyId,
                Name        = request.Name,
                City        = request.City,
                Address     = request.Address,
                Phone       = request.Phone,
                ManagerName = request.ManagerName,
                IsActive    = true,
                CreatedAt   = DateTime.UtcNow
            };

            _context.CompanyBranches.Add(branch);
            await _context.SaveChangesAsync(ct);
            return branch.Id;
        }

        public async Task<(bool Success, string Message)> UpdateBranchAsync(
            int branchId, int companyId, UpdateBranchRequest request, CancellationToken ct = default)
        {
            var branch = await _context.CompanyBranches
                .FirstOrDefaultAsync(b => b.Id == branchId && b.CompanyId == companyId, ct);

            if (branch == null)
                return (false, "Filial topilmadi yoki sizga tegishli emas");

            if (request.Name        != null) branch.Name        = request.Name;
            if (request.City        != null) branch.City        = request.City;
            if (request.Address     != null) branch.Address     = request.Address;
            if (request.Phone       != null) branch.Phone       = request.Phone;
            if (request.ManagerName != null) branch.ManagerName = request.ManagerName;
            if (request.IsActive    != null) branch.IsActive    = request.IsActive.Value;

            await _context.SaveChangesAsync(ct);
            return (true, "Filial yangilandi");
        }

        public async Task<(bool Success, string Message)> DeleteBranchAsync(
            int branchId, int companyId, CancellationToken ct = default)
        {
            var branch = await _context.CompanyBranches
                .FirstOrDefaultAsync(b => b.Id == branchId && b.CompanyId == companyId, ct);

            if (branch == null)
                return (false, "Filial topilmadi yoki sizga tegishli emas");

            _context.CompanyBranches.Remove(branch);
            await _context.SaveChangesAsync(ct);
            return (true, "Filial o'chirildi");
        }

        public async Task<List<CompanyDocumentDto>> GetDocumentsAsync(
            int companyId, CancellationToken ct = default)
        {
            return await _context.CompanyDocuments
                .AsNoTracking()
                .Where(d => d.CompanyId == companyId)
                .Select(d => new CompanyDocumentDto
                {
                    Id         = d.Id,
                    Type       = d.Type,
                    FileName   = d.FileName,
                    FileUrl    = d.FileUrl,
                    UploadedAt = d.UploadedAt,
                    ExpiryDate = d.ExpiryDate
                })
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync(ct);
        }

        public async Task<int> UploadDocumentAsync(
            int companyId, UploadDocumentRequest request, CancellationToken ct = default)
        {
            var doc = new CompanyDocument
            {
                CompanyId  = companyId,
                Type       = request.DocumentType,
                FileName   = request.FileName,
                FileUrl    = request.FileUrl,
                UploadedAt = DateTime.UtcNow,
                ExpiryDate = request.ExpiryDate
            };

            _context.CompanyDocuments.Add(doc);
            await _context.SaveChangesAsync(ct);
            return doc.Id;
        }

        public async Task<(bool Success, string Message)> DeleteDocumentAsync(
            int documentId, int companyId, CancellationToken ct = default)
        {
            var doc = await _context.CompanyDocuments
                .FirstOrDefaultAsync(d => d.Id == documentId && d.CompanyId == companyId, ct);

            if (doc == null)
                return (false, "Hujjat topilmadi yoki sizga tegishli emas");

            _context.CompanyDocuments.Remove(doc);
            await _context.SaveChangesAsync(ct);
            return (true, "Hujjat o'chirildi");
        }
    }
}
