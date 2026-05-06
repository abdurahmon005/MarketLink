using MarketLink.Domain.Enums;

namespace MarketLink.Application.Models.Company
{
    public class CompanyProfileResponse
    {
        public int Id { get; set; }
        public string FounderName { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public CompanyDirection ProductionType { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public string? CertificateUrl { get; set; }
        public double AverageRating { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
