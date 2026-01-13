using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.UserOtp
{
    public class OtpVerificationModel
    {
        public string? Email { get; set; }
        public string? Phonenumber { get; set; }
        public string Code { get; set; }

    }
}
