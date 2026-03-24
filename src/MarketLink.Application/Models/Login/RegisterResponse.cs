using MarketLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.Login
{
    public class RegisterResponse
    {
        public Guid UserId { get; set; }
        public string PhoneNumber { get; set; }
        public UserStatus Status { get; set; }
        public string Message { get; set; }
    }
}
