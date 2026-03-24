using MarketLink.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

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
            _issuer    = cfg["Jwt:Issuer"]    ?? "MarketLink";
            _audience  = cfg["Jwt:Audience"]  ?? "MarketLink";
        }

        public string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.MobilePhone, user.PhoneNumber),
                new("status", user.Status.ToString())
            };

            if (user.UserRoles != null)
                foreach (var ur in user.UserRoles)
                    claims.Add(new Claim(ClaimTypes.Role, ur.Role.Name));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

            var descriptor = new SecurityTokenDescriptor
            {
                Subject            = new ClaimsIdentity(claims),
                Expires            = DateTime.UtcNow.AddHours(1),
                Issuer             = _issuer,
                Audience           = _audience,
                SigningCredentials  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(handler.CreateToken(descriptor));
        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var principal = handler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidIssuer              = _issuer,
                    ValidateAudience         = true,
                    ValidAudience            = _audience,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                    ClockSkew                = TimeSpan.Zero
                }, out _);

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public Guid? GetUserIdFromToken(string token)
        {
            var principal = ValidateToken(token);
            var value = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
