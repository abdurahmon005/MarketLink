using MarketLink.API.Common;
using MarketLink.Application.Models.Product;
using MarketLink.Application.Models.Supplier;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/supplier/products")]
    [Authorize(Roles = "Company,Admin")]
    public class SupplierProductController : ControllerBase
    {
        private readonly ICompanyProductService _productService;
        private readonly ISupplierNotificationService _notificationService;
        private readonly ILogger<SupplierProductController> _logger;

        public SupplierProductController(
            ICompanyProductService productService,
            ISupplierNotificationService notificationService,
            ILogger<SupplierProductController> logger)
        {
            _productService      = productService;
            _notificationService = notificationService;
            _logger              = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(
            [FromQuery] SupplierProductFilter filter, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var result = await _productService.GetProductsAsync(companyId.Value, filter, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Mahsulotlar ro'yxati", Data = result });
        }

        [HttpGet("top")]
        public async Task<IActionResult> GetTopProducts(
            [FromQuery] string period = "week", CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var result = await _productService.GetTopProductsAsync(companyId.Value, period, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Top mahsulotlar", Data = result });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProduct(int id, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var result = await _productService.GetProductDetailAsync(id, companyId.Value, ct);
            if (result == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Mahsulot topilmadi" });

            return Ok(new ApiResponse<object> { Success = true, Message = "Mahsulot ma'lumotlari", Data = result });
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(
            [FromForm] CreateProductRequest request, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ValidationError());

            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var productId = await _productService.CreateProductAsync(companyId.Value, request, ct);
            if (productId <= 0)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Mahsulot yaratishda xato" });

            // Check low stock
            if (request.StockQuantity < 10)
                await _notificationService.CheckAndSendLowStockAlertsAsync(companyId.Value, ct);

            return CreatedAtAction(nameof(GetProduct), new { id = productId },
                new ApiResponse<object> { Success = true, Message = "Mahsulot yaratildi", Data = new { id = productId } });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(
            int id, [FromBody] UpdateProductRequest request, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var (success, message) = await _productService.UpdateProductAsync(id, companyId.Value, request, ct);
            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var (success, message) = await _productService.DeleteProductAsync(id, companyId.Value, ct);
            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        [HttpPatch("{id:int}/toggle-active")]
        public async Task<IActionResult> ToggleActive(int id, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var (success, message) = await _productService.ToggleActiveAsync(id, companyId.Value, ct);
            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        [HttpPut("{id:int}/stock")]
        public async Task<IActionResult> UpdateStock(
            int id, [FromBody] UpdateStockRequest request, CancellationToken ct = default)
        {
            if (!ModelState.IsValid)
                return BadRequest(ValidationError());

            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var (success, message) = await _productService.UpdateStockAsync(
                id, companyId.Value, request, userId.Value, ct);

            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            await _notificationService.CheckAndSendLowStockAlertsAsync(companyId.Value, ct);

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        [HttpPost("{id:int}/image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateImage(
            int id, [FromForm] IFormFile file, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var (success, message, imageUrl) = await _productService.UpdateImageAsync(
                id, companyId.Value, file, ct);

            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = message,
                Data    = new { imageUrl }
            });
        }

        private int? GetCompanyId()
        {
            var value = User.FindFirstValue("profile_id");
            return int.TryParse(value, out var id) ? id : null;
        }

        private Guid? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }

        private ApiResponse<object> ValidationError() => new()
        {
            Success = false,
            Message = "Ma'lumotlar noto'g'ri",
            Errors  = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList()
        };
    }
}
