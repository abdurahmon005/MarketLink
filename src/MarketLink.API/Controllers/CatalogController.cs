using MarketLink.Application.Models.Catalog;
using MarketLink.Application.Models.Common;
using MarketLink.Application.Service;
using MarketLink.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogService _catalogService;

        public CatalogController(ICatalogService catalogService)
            => _catalogService = catalogService;

        /// <summary>Mahsulotlar katalogi (filter + sahifalash)</summary>
        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] int? companyId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] CompanyDirection? direction,
            [FromQuery] string? search,
            [FromQuery] string sortBy = "createdAt",
            [FromQuery] string sortOrder = "desc",
            [FromQuery] int page = 1,
            [FromQuery] int size = 20,
            CancellationToken ct = default)
        {
            var filter = new CatalogFilterDto
            {
                CompanyId = companyId,
                MinPrice  = minPrice,
                MaxPrice  = maxPrice,
                Direction = direction,
                Search    = search,
                SortBy    = sortBy,
                SortOrder = sortOrder,
                Page      = page,
                Size      = size
            };

            var result = await _catalogService.GetProductsAsync(filter, ct);
            return Ok(ApiResponse<PagedResult<CatalogProductDto>>.Ok(result, "Katalog mahsulotlari"));
        }

        /// <summary>Bitta mahsulot tafsiloti</summary>
        [HttpGet("products/{id:int}")]
        public async Task<IActionResult> GetProductById(int id, CancellationToken ct)
        {
            var product = await _catalogService.GetProductByIdAsync(id, ct);
            if (product == null)
                return NotFound(ApiResponse<object>.Fail("Mahsulot topilmadi"));

            return Ok(ApiResponse<CatalogProductDetailDto>.Ok(product, "Mahsulot tafsiloti"));
        }
    }
}
