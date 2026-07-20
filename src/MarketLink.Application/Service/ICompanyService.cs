using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Company;
using MarketLink.Application.Models.Supplier;
using MarketLink.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace MarketLink.Application.Service
{
    public interface ICompanyService
    {
        Task<PagedResult<CompanyProfileResponse>> GetAllAsync(
            int page, int pageSize, CompanyDirection? type = null, CancellationToken ct = default);

        Task<CompanyProfileResponse?> GetByIdAsync(int id, CancellationToken ct = default);

        Task<CompanyProfileResponse?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

        Task<(bool Success, string Message)> UpdateAsync(
            Guid userId, UpdateCompanyProfileRequest request, CancellationToken ct = default);

        // ── Supplier Panel ────────────────────────────────────────────────────

        Task<CompanyProfileDto?> GetProfileAsync(int companyId, CancellationToken ct = default);

        Task<(bool Success, string Message)> UpdateProfileAsync(
            int companyId, UpdateCompanyRequest request, CancellationToken ct = default);

        Task<(bool Success, string Message, string? LogoUrl)> UpdateLogoAsync(
            int companyId, IFormFile file, CancellationToken ct = default);

        Task<List<CompanyBranchDto>> GetBranchesAsync(int companyId, CancellationToken ct = default);

        Task<int> AddBranchAsync(int companyId, CreateBranchRequest request, CancellationToken ct = default);

        Task<(bool Success, string Message)> UpdateBranchAsync(
            int branchId, int companyId, UpdateBranchRequest request, CancellationToken ct = default);

        Task<(bool Success, string Message)> DeleteBranchAsync(
            int branchId, int companyId, CancellationToken ct = default);

        Task<List<CompanyDocumentDto>> GetDocumentsAsync(int companyId, CancellationToken ct = default);

        Task<int> UploadDocumentAsync(int companyId, UploadDocumentRequest request, CancellationToken ct = default);

        Task<(bool Success, string Message)> DeleteDocumentAsync(
            int documentId, int companyId, CancellationToken ct = default);
    }
}
