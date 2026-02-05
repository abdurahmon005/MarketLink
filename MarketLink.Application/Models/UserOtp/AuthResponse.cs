using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.UserOtp
{
    public class AuthResponse
    {

        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        //public UserDto User { get; set; }
    }
}
