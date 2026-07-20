using MarketLink.Application.Models.Common;
using MarketLink.Application.Models.Product;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Authorize(Roles = "Company,Admin")]
    public class ProductsController : ControllerBase
    {
        private readonly ICompanyProductService _productService;

        public ProductsController(ICompanyProductService productService)
            => _productService = productService;

        /// <summary>Korxonaning o'z mahsulotlari ro'yxati</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var result = await _productService.GetMyProductsAsync(companyId.Value, page, pageSize, ct);
            return Ok(ApiResponse<PagedResult<ProductResponse>>.Ok(result, "Mahsulotlar ro'yxati"));
        }

        /// <summary>Yangi mahsulot yaratish</summary>
        [HttpPost]
        public async Task<IActionResult> CreateProduct(
            [FromBody] CreateProductRequest request, CancellationToken ct)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var (success, message, data) = await _productService.CreateAsync(companyId.Value, request, ct);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(message));

            return Ok(ApiResponse<ProductResponse>.Ok(data!, message));
        }

        /// <summary>Mahsulotni yangilash</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(
            int id, [FromBody] UpdateProductRequest request, CancellationToken ct)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var (success, message) = await _productService.UpdateAsync(id, companyId.Value, request, ct);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(message));

            return Ok(ApiResponse<object>.Ok(null!, message));
        }

        /// <summary>Mahsulotni o'chirish</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken ct)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var (success, message) = await _productService.DeleteAsync(id, companyId.Value, ct);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(message));

            return Ok(ApiResponse<object>.Ok(null!, message));
        }

        /// <summary>Mahsulot rasmini yangilash</summary>
        [HttpPut("{id:int}/image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProductImage(
            int id, IFormFile image, CancellationToken ct)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var (success, message, imageUrl) = await _productService.UpdateImageAsync(id, companyId.Value, image, ct);
            if (!success)
                return BadRequest(ApiResponse<object>.Fail(message));

            return Ok(ApiResponse<object>.Ok(new { imageUrl }, message));
        }

        /// <summary>Mahsulot qoldiqlari monitoringi</summary>
        [HttpGet("stock")]
        public async Task<IActionResult> GetStock(CancellationToken ct)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var result = await _productService.GetStockAsync(companyId.Value, ct);
            return Ok(ApiResponse<List<ProductStockResponse>>.Ok(result, "Qoldiqlar"));
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private int? GetProfileId()
        {
            var v = User.FindFirstValue("profile_id");
            return int.TryParse(v, out var id) ? id : null;
        }
    }
}
