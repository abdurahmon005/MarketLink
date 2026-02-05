using MarketLink.Application.Models.Company;
using MarketLink.Application.Models.Shop;
using MarketLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.User
{
    public class UserProfileResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public UserStatus Status { get; set; }
        public bool IsEmailVerified { get; set; }
        public List<string> Roles { get; set; } // ["Company"] / ["Shop"]
        public CompanyProfileResponse? Company { get; set; }
        public ShopProfileResponse? Shop { get; set; }
    }
}
