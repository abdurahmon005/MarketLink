using Microsoft.AspNetCore.Http;

namespace MarketLink.Application.Models.Product
{
    public class UpdateProductImageRequest
    {
        public IFormFile? Image { get; set; }
    }
}
