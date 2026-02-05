using MarketLink.Domain.Entities;
using MarketLink.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MarketLink.Application.Service.Impl
{
    public class JwtService : IJwtService
    {
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtService(IConfiguration cfg)
        {
            _secretKey = cfg["Jwt:SecretKey"] ?? throw new ArgumentNullException("Jwt:SecretKey");
            _issuer = cfg["Jwt:Issuer"] ?? "MarketLink";
            _audience = cfg["Jwt:Audience"] ?? "MarketLink";
        }
        public string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new("status", user.Status.ToString())
            };

            // Barcha rol-lar claim sifatida qo'shiladi
            if (user.UserRoles != null)
            {
                foreach (var ur in user.UserRoles)
                    claims.Add(new Claim(ClaimTypes.Role, ur.Role.Name));
            }

            var key = Encoding.UTF8.GetBytes(_secretKey);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(descriptor));
        }

        public string GenerateRefreshToken()
        {
            throw new NotImplementedException();
        }

        public Guid? GetUserIdFromToken(string token)
        {
            throw new NotImplementedException();
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            throw new NotImplementedException();
        }
    }
}
