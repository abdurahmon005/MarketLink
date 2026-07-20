using MarketLink.Domain.Enums;

namespace MarketLink.Application.Models.Supplier
{
    public class CompanyProfileDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string FounderName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public CompanyDirection AccountType { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public CompanyStatsDto Stats { get; set; } = new();
    }

    public class CompanyStatsDto
    {
        public int TotalOrders { get; set; }
        public double AvgRating { get; set; }
        public int ActiveShops { get; set; }
    }

    public class CompanyBranchDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int ActiveOrdersCount { get; set; }
    }

    public class CompanyDocumentDto
    {
        public int Id { get; set; }
        public DocumentType Type { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    }

    public class CreateBranchRequest
    {
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
    }

    public class UpdateBranchRequest
    {
        public string? Name { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? ManagerName { get; set; }
        public bool? IsActive { get; set; }
    }

    public class UploadDocumentRequest
    {
        public DocumentType DocumentType { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
    }

    public class UpdateCompanyRequest
    {
        public string? CompanyName { get; set; }
        public string? FounderName { get; set; }
        public string? Address { get; set; }
        public string? Description { get; set; }
    }
}
