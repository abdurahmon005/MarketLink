using MarketLink.Domain.Entities;
using System.Security.Claims;

namespace MarketLink.Application.Service
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user, int? profileId = null);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateToken(string token);
        Guid? GetUserIdFromToken(string token);
    }
}
