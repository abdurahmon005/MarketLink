using MarketLink.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MarketLink.Application.Models.Company
{
    public class RegisterCompanyRequest
    {
        [Required] public string PhoneNumber    { get; set; } = default!;
        [Required, MinLength(6)] public string Password { get; set; } = default!;
        [Required] public string FounderName    { get; set; } = default!;
        [Required] public string CompanyName    { get; set; } = default!;
        [Required] public string Address        { get; set; } = default!;
        [Required] public CompanyDirection ProductionType { get; set; }
        public string? Description { get; set; }
    }
}
