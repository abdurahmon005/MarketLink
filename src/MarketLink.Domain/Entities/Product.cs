namespace MarketLink.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        /// <summary>Narx (so'm)</summary>
        public decimal Price { get; set; }

        /// <summary>Bir o'ramda nechta mahsulot bor</summary>
        public int PackageSize { get; set; }

        /// <summary>Ombordagi qoldiq (o'ram soni)</summary>
        public int StockQuantity { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>Barcha reytinglarning o'rtachasi (cache — RatingService tomonidan yangilanadi)</summary>
        public double AverageRating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Company Company { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    }
}
