using FluentValidation;
using MarketLink.API.Common;
using MarketLink.Application.Models.Product;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers.Company
{
    /// <summary>Company mahsulot boshqaruvi</summary>
    [ApiController]
    [Route("api/products")]
    [Authorize(Roles = "Company")]
    public class CompanyProductsController : ControllerBase
    {
        private readonly ICompanyProductService _productService;
        private readonly IValidator<CreateProductRequest> _createValidator;
        private readonly IValidator<UpdateProductRequest> _updateValidator;

        public CompanyProductsController(
            ICompanyProductService productService,
            IValidator<CreateProductRequest> createValidator,
            IValidator<UpdateProductRequest> updateValidator)
        {
            _productService  = productService;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
        }

        /// <summary>Yangi mahsulot qo'shish</summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create(
            [FromForm] CreateProductRequest request, CancellationToken ct)
        {
            var validation = await _createValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return BadRequest(ValidationFail(validation));

            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var (success, message, data) =
                await _productService.CreateAsync(companyId.Value, request, ct);

            if (!success)
                return BadRequest(Fail(message));

            return StatusCode(201, new ApiResponse<object>
            {
                Success = true,
                Message = message,
                Data    = data
            });
        }

        /// <summary>O'z mahsulotlari ro'yxati</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMy(
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (size is < 1 or > 50) size = 10;

            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var result = await _productService.GetMyProductsAsync(companyId.Value, page, size, ct);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Mahsulotlar ro'yxati",
                Data    = result
            });
        }

        /// <summary>Bitta mahsulot tafsiloti</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var product = await _productService.GetByIdAsync(id, companyId.Value, ct);
            if (product == null)
                return NotFound(Fail("Mahsulot topilmadi yoki sizga tegishli emas"));

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Mahsulot",
                Data    = product
            });
        }

        /// <summary>Mahsulotni tahrirlash</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateProductRequest request,
            CancellationToken ct)
        {
            var validation = await _updateValidator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return BadRequest(ValidationFail(validation));

            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var (success, message) =
                await _productService.UpdateAsync(id, companyId.Value, request, ct);

            if (!success)
                return BadRequest(Fail(message));

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        /// <summary>Mahsulotni o'chirish</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var (success, message) =
                await _productService.DeleteAsync(id, companyId.Value, ct);

            if (!success)
                return BadRequest(Fail(message));

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        /// <summary>Mahsulot rasmini yangilash</summary>
        [HttpPut("{id:int}/image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateImage(
            int id,
            [FromForm] UpdateProductImageRequest request,
            CancellationToken ct)
        {
            var image = request.Image;
            if (image == null || image.Length == 0)
                return BadRequest(Fail("Rasm tanlanmagan"));

            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var (success, message, imageUrl) =
                await _productService.UpdateImageAsync(id, companyId.Value, image, ct);

            if (!success)
                return BadRequest(Fail(message));

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = message,
                Data    = new { imageUrl }
            });
        }

        /// <summary>Qoldiq monitoringi</summary>
        [HttpGet("stock")]
        public async Task<IActionResult> GetStock(CancellationToken ct)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var stock = await _productService.GetStockAsync(companyId.Value, ct);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Qoldiq ma'lumotlari",
                Data    = stock
            });
        }

        /// <summary>Mahsulot reytinglari ro'yxati</summary>
        [HttpGet("ratings")]
        public async Task<IActionResult> GetRatings(
            [FromQuery] int? productId,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10,
            CancellationToken ct = default)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var result = await _productService.GetRatingsAsync(
                companyId.Value, productId, page, size, ct);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Reytinglar",
                Data    = result
            });
        }

        /// <summary>Bitta mahsulotning o'rtacha reytingi</summary>
        [HttpGet("{id:int}/rating")]
        public async Task<IActionResult> GetProductRating(int id, CancellationToken ct)
        {
            var companyId = GetProfileId();
            if (companyId == null) return Forbid();

            var avg = await _productService.GetAverageRatingAsync(id, companyId.Value, ct);
            if (avg == null)
                return NotFound(Fail("Mahsulot topilmadi yoki sizga tegishli emas"));

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "O'rtacha reyting",
                Data    = avg
            });
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private int? GetProfileId()
        {
            var v = User.FindFirstValue("profile_id");
            return int.TryParse(v, out var id) ? id : null;
        }

        private static ApiResponse<object> Fail(string message) =>
            new() { Success = false, Message = message };

        private static ApiResponse<object> ValidationFail(
            FluentValidation.Results.ValidationResult v) => new()
        {
            Success = false,
            Message = "Validatsiya xatosi",
            Errors  = v.Errors.Select(e => e.ErrorMessage).ToList()
        };
    }
}
