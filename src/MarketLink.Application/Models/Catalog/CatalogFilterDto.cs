using MarketLink.Domain.Enums;

namespace MarketLink.Application.Models.Catalog
{
    public class CatalogFilterDto
    {
        public int? CompanyId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public CompanyDirection? Direction { get; set; }
        public string? Search { get; set; }

        /// <summary>price | rating | name | createdAt</summary>
        public string SortBy { get; set; } = "createdAt";

        /// <summary>asc | desc</summary>
        public string SortOrder { get; set; } = "desc";

        public int Page { get; set; } = 1;
        public int Size { get; set; } = 20;
    }
}
