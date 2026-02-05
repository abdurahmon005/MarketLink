using MarketLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.Shop
{
    public class RegisterShopRequest
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;

        public string FounderName { get; set; } = default!;
        public string ShopName { get; set; } = default!;
        public string Address { get; set; } = default!;
        public ShopType ShopType { get; set; }
        public string Description { get; set; } = default!;
        public string LogoUrl { get; set; } = default!;
        public string SertificateUrl { get; set; } = default!;
    }
}
