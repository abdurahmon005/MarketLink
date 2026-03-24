using MarketLink.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MarketLink.Application.Models.Shop
{
    public class RegisterShopRequest
    {
        [Required] public string PhoneNumber { get; set; } = default!;
        [Required, MinLength(6)] public string Password { get; set; } = default!;
        [Required] public string FounderName { get; set; } = default!;
        [Required] public string ShopName    { get; set; } = default!;
        [Required] public string Address     { get; set; } = default!;
        [Required] public ShopType ShopType  { get; set; }
        public string? Description { get; set; }
    }
}
