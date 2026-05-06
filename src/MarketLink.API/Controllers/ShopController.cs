using MarketLink.API.Common;
using MarketLink.Application.Service;
using MarketLink.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace MarketLink.API.Controllers
{
    /// <summary>Do'konlar umumiy ro'yxati (hamma ko'ra oladi)</summary>
    [ApiController]
    [Route("api/shops")]
    public class ShopController : ControllerBase
    {
        private readonly IShopService _shopService;

        public ShopController(IShopService shopService)
            => _shopService = shopService;

        /// <summary>Barcha do'konlar ro'yxati (sahifalash + tur bo'yicha filter)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] ShopType? type = null,
            CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 50) pageSize = 10;

            var result = await _shopService.GetAllAsync(page, pageSize, type, ct);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Do'konlar ro'yxati",
                Data    = result
            });
        }

        /// <summary>ID bo'yicha do'konni olish</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var shop = await _shopService.GetByIdAsync(id, ct);
            if (shop == null)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Do'kon topilmadi"
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Do'kon",
                Data    = shop
            });
        }
    }
}
