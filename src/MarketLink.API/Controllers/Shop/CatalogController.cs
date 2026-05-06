using MarketLink.API.Common;
using MarketLink.Application.Models.Catalog;
using MarketLink.Application.Service;
using MarketLink.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarketLink.API.Controllers.Shop
{
    /// <summary>Mahsulotlar katalogi (faqat Shop roli ko'radi)</summary>
    [ApiController]
    [Route("api/catalog")]
    [Authorize(Roles = "Shop")]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogService _catalogService;

        public CatalogController(ICatalogService catalogService)
            => _catalogService = catalogService;

        /// <summary>
        /// Barcha mahsulotlar ro'yxati.
        /// Filter: companyId, minPrice, maxPrice, direction, search
        /// Sort: sortBy (price|rating|name|createdAt), sortOrder (asc|desc)
        /// </summary>
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

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Katalog",
                Data    = result
            });
        }

        /// <summary>Bitta mahsulot tafsiloti (tavsif + reytinglar)</summary>
        [HttpGet("products/{productId:int}")]
        public async Task<IActionResult> GetProduct(int productId, CancellationToken ct)
        {
            var product = await _catalogService.GetProductByIdAsync(productId, ct);
            if (product == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Mahsulot topilmadi"
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Mahsulot tafsiloti",
                Data    = product
            });
        }
    }
}
