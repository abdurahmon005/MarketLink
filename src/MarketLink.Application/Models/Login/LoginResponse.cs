using MarketLink.Application.Models.User;
using MarketLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.Login
{
    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public UserStatus Status { get; set; }
        public bool IsPhoneVerified { get; set; }
        public UserProfileResponse Profile { get; set; }
    }
}
