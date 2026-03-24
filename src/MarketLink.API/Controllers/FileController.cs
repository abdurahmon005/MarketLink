using MarketLink.API.Common;
using MarketLink.Application.Service;
using MarketLink.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    [ApiController]
    [Route("api/files")]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<FileController> _logger;

        public FileController(IFileService fileService, ILogger<FileController> logger)
        {
            _fileService = fileService;
            _logger      = logger;
        }
 
        [HttpPost("logo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadLogo(IFormFile file, CancellationToken ct)
        {
            return await UploadFile(file, FileType.Logo, ct);
        }

        /// <summary>Sertifikat yuklash</summary>
        [HttpPost("certificate")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadCertificate(IFormFile file, CancellationToken ct)
        {
            return await UploadFile(file, FileType.Certificate, ct);
        }

        [HttpPost("product-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProductImage(IFormFile file, CancellationToken ct)
        {
            return await UploadFile(file, FileType.ProductImage, ct);
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload(IFormFile file, [FromQuery] FileType type, CancellationToken ct)
        {
            return await UploadFile(file, type, ct);
        }


        [HttpGet("presigned")]
        public async Task<IActionResult> GetPresignedUrl(
            [FromQuery] string objectPath,
            [FromQuery] int    expirySeconds = 3600,
            CancellationToken  ct = default)
        {
            if (string.IsNullOrWhiteSpace(objectPath))
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "objectPath bo'sh bo'lmasligi kerak"
                });

            try
            {
                var url = await _fileService.GetPresignedUrlAsync(objectPath, expirySeconds, ct);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Presigned URL",
                    Data    = new { Url = url, ExpiresInSeconds = expirySeconds }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Presigned URL olishda xatolik: {Path}", objectPath);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "URL olishda xatolik yuz berdi"
                });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFile(
            [FromQuery] string   objectPath,
            [FromQuery] FileType type,
            CancellationToken    ct = default)
        {
            if (string.IsNullOrWhiteSpace(objectPath))
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "objectPath bo'sh bo'lmasligi kerak"
                });

            var result = await _fileService.DeleteAsync(objectPath, type, ct);

            if (!result)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Fayl topilmadi yoki o'chirishda xatolik"
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Fayl muvaffaqiyatli o'chirildi"
            });
        }
        private async Task<IActionResult> UploadFile(IFormFile file, FileType fileType, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Fayl tanlanmagan"
                });

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            try
            {
                var result = await _fileService.UploadAsync(file, fileType, userId.Value, ct);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Fayl muvaffaqiyatli yuklandi",
                    Data    = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fayl yuklashda xatolik: {FileName}", file.FileName);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Fayl yuklashda xatolik yuz berdi"
                });
            }
        }

        private Guid? GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
