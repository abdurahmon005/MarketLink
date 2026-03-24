using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Models.UserOtp
{
    public class VerifyPhoneNumberRequest
    {
        public string PhoneNumber { get; set; }
        public string OtpCode { get; set; }
    }
}
