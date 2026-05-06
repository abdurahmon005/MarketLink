using Microsoft.AspNetCore.Http;

namespace MarketLink.Application.Models.Shop
{
    public class UpdateShopMediaRequest
    {
        public IFormFile? Logo { get; set; }
        public IFormFile? Certificate { get; set; }
    }
}
