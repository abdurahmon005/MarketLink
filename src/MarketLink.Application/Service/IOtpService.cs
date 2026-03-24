using MarketLink.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Service
{
    public interface IOtpService
    {
        Task<string> GenerateAndSaveOtpAsync(Guid userId);
        Task<UserOTPs?> GetLatestOtpAsync(Guid userId, string code);
        Task<bool> SentOtpAsync(string phoneNumber);
        Task<(bool isValid, User user)> VerifyOtpAsync(string phoneNumber, string code);
        Task<string?> GetOtpFromCacheAsync(string phoneNumber);
    }
}
