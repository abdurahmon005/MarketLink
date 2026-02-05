using MarketLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.Shop
{
    public class ShopProfileResponse
    {
        public int Id { get; set; }
        public string FounderName { get; set; }
        public string ShopName { get; set; }
        public string Address { get; set; }
        public ShopType ShopType { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string SertificateUrl { get; set; }
    }
}
