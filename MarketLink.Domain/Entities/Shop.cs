using MarketLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Domain.Entities
{
    public class Shop
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }

        public string FounderName { get; set; }
        public string ShopName { get; set; }
        public string Address { get; set; }
        public ShopType ShopType { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string SertificateUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; }

    }
}
