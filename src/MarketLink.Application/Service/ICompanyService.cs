using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Company;
using MarketLink.Domain.Enums;

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
    }
}
