using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.UserOtp
{
    public class ResetPasswordRequest
    {
        public string PhoneNumber { get; set; }
        public string ResetCode { get; set; }
        public string NewPassword { get; set; }
    }
}
