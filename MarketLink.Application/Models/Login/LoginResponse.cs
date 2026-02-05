using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.Login
{
    public class LoginResponse
    {
        public TokenResponse Token { get; set; } = default!;
        public Guid UserId { get; set; }
        public string Email { get; set; } = default!;
    }
}
