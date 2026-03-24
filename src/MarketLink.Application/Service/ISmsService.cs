using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Service
{
    public interface ISmsService
    {
        Task<bool> SendOtpSmsAsync(string phoneNumber, string otpCode);
        Task<bool> SendPasswordResetSmsAsync(string phoneNumber, string resetCode);
        Task<bool> SendWelcomeSmsAsync(string phoneNumber, string userName);
        Task<bool> SendSmsAsync(string phoneNumber, string message);
    }
}
