using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.User
{
    public class UserResponseModel
    {
        public Guid Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
    }
}
