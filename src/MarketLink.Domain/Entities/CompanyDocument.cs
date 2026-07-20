using MarketLink.Domain.Enums;

namespace MarketLink.Domain.Entities
{
    public class CompanyDocument
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public DocumentType Type { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; }

        // Navigation
        public Company Company { get; set; } = null!;
    }
}
