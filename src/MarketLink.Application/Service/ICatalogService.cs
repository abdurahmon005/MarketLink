using MarketLink.Application.Models.Catalog;
using MarketLink.Application.Models.Common;

namespace MarketLink.Application.Service
{
    public interface ICatalogService
    {
        Task<PagedResult<CatalogProductDto>> GetProductsAsync(
            CatalogFilterDto filter, CancellationToken ct = default);

        Task<CatalogProductDetailDto?> GetProductByIdAsync(
            int productId, CancellationToken ct = default);
    }
}
