using MarketLink.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
 

namespace MarketLink.Application.Helpers.GenerateJWT
{
    public class JwtService
    {
        private readonly JwtOption _jwtOption;

        public JwtService(IOptions<JwtOption> jwtOption)
        {
            _jwtOption = jwtOption.Value;
        }
        public string GenerateToken(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    };

            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
            }


            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtOption.SecretKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOption.Issuer,
                audience: _jwtOption.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(_jwtOption.ExpirationInSeconds),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
