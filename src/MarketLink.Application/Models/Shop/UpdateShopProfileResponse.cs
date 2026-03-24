using MarketLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.Shop
{
    public class UpdateShopProfileResponse
    {
        public string? TasischiFISH { get; set; }
        public string? DokonNomi { get; set; }
        public string? Manzil { get; set; }
        public ShopType? FaoliyatTuri { get; set; }
        public string? DokonTavsifi { get; set; }
    }
}
