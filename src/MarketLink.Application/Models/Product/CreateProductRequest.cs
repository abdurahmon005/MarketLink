using Microsoft.AspNetCore.Http;

namespace MarketLink.Application.Models.Product
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
        public decimal Price { get; set; }

        /// <summary>Bir o'ramda nechta mahsulot</summary>
        public int PackageSize { get; set; }

        /// <summary>Boshlang'ich qoldiq (o'ram soni)</summary>
        public int StockQuantity { get; set; }
    }
}
