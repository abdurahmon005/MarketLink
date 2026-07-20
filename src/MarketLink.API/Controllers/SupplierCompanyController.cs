using MarketLink.API.Common;
using MarketLink.Application.Models.Supplier;
using MarketLink.Application.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketLink.API.Controllers
{
    public class UploadDocumentFormRequest
    {
        public IFormFile File { get; set; } = null!;
        public MarketLink.Domain.Enums.DocumentType DocumentType { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    [ApiController]
    [Route("api/supplier/company")]
    [Authorize(Roles = "Company,Admin")]
    public class SupplierCompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly IFileService _fileService;

        public SupplierCompanyController(ICompanyService companyService, IFileService fileService)
        {
            _companyService = companyService;
            _fileService    = fileService;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile(CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var result = await _companyService.GetProfileAsync(companyId.Value, ct);
            if (result == null)
                return NotFound(new ApiResponse<object> { Success = false, Message = "Kompaniya profili topilmadi" });

            return Ok(new ApiResponse<object> { Success = true, Message = "Kompaniya profili", Data = result });
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateCompanyRequest request, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var (success, message) = await _companyService.UpdateProfileAsync(companyId.Value, request, ct);
            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        [HttpPut("logo")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateLogo([FromForm] IFormFile file, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var (success, message, logoUrl) = await _companyService.UpdateLogoAsync(companyId.Value, file, ct);
            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message, Data = new { logoUrl } });
        }

        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches(CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var branches = await _companyService.GetBranchesAsync(companyId.Value, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Filiallar ro'yxati", Data = branches });
        }

        [HttpPost("branches")]
        public async Task<IActionResult> AddBranch(
            [FromBody] CreateBranchRequest request, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var branchId = await _companyService.AddBranchAsync(companyId.Value, request, ct);
            return Created($"api/supplier/company/branches/{branchId}",
                new ApiResponse<object> { Success = true, Message = "Filial qo'shildi", Data = new { id = branchId } });
        }

        [HttpPut("branches/{id:int}")]
        public async Task<IActionResult> UpdateBranch(
            int id, [FromBody] UpdateBranchRequest request, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var (success, message) = await _companyService.UpdateBranchAsync(id, companyId.Value, request, ct);
            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        [HttpDelete("branches/{id:int}")]
        public async Task<IActionResult> DeleteBranch(int id, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var (success, message) = await _companyService.DeleteBranchAsync(id, companyId.Value, ct);
            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message });
        }

        [HttpGet("documents")]
        public async Task<IActionResult> GetDocuments(CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var docs = await _companyService.GetDocumentsAsync(companyId.Value, ct);
            return Ok(new ApiResponse<object> { Success = true, Message = "Hujjatlar ro'yxati", Data = docs });
        }

        [HttpPost("documents")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadDocument(
            [FromForm] UploadDocumentFormRequest form,
            CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            if (form.File == null || form.File.Length == 0)
                return BadRequest(new ApiResponse<object> { Success = false, Message = "Fayl tanlanmagan" });

            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var upload = await _fileService.UploadAsync(form.File, Domain.Enums.FileType.Certificate, userId.Value, ct);

            var request = new UploadDocumentRequest
            {
                DocumentType = form.DocumentType,
                FileName     = form.File.FileName,
                FileUrl      = upload.Url ?? upload.ObjectPath,
                ExpiryDate   = form.ExpiryDate
            };

            var docId = await _companyService.UploadDocumentAsync(companyId.Value, request, ct);

            return Created($"api/supplier/company/documents/{docId}",
                new ApiResponse<object>
                {
                    Success = true,
                    Message = "Hujjat yuklandi",
                    Data    = new { id = docId, fileUrl = request.FileUrl }
                });
        }

        [HttpDelete("documents/{id:int}")]
        public async Task<IActionResult> DeleteDocument(int id, CancellationToken ct = default)
        {
            var companyId = GetCompanyId();
            if (companyId == null) return Unauthorized();

            var (success, message) = await _companyService.DeleteDocumentAsync(id, companyId.Value, ct);
            if (!success)
                return BadRequest(new ApiResponse<object> { Success = false, Message = message });

            return Ok(new ApiResponse<object> { Success = true, Message = message });
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
    }
}
