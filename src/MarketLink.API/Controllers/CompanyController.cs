using MarketLink.API.Common;
using MarketLink.Application.Service;
using MarketLink.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MarketLink.API.Controllers
{
    /// <summary>Kompaniyalar umumiy ro'yxati (hamma ko'ra oladi)</summary>
    [ApiController]
    [Route("api/companies")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
            => _companyService = companyService;

        /// <summary>Barcha kompaniyalar ro'yxati (sahifalash + tur bo'yicha filter)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] CompanyDirection? type = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 50) pageSize = 10;

            var result = await _companyService.GetAllAsync(page, pageSize, type, ct);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Kompaniyalar ro'yxati",
                Data    = result
            });
        }

        /// <summary>ID bo'yicha kompaniyani olish</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var company = await _companyService.GetByIdAsync(id, ct);
            if (company == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Kompaniya topilmadi"
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Kompaniya",
                Data    = company
            });
        }
    }
}
