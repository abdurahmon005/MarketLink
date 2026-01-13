using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.User
{
    public class UpdateUserPassword
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }

    }
}
