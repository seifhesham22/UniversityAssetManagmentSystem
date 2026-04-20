using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UAMS.Identity.IdentityModels;

namespace UAMS.Identity.Services.TokenService
{
    public class TokenHandler : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenHandler(IConfiguration config) => _config = config;

        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var claims = new List<Claim>
            {
                new(AppClaims.UserId, user.Id.ToString()),
                new(AppClaims.Role, user.Role.ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:issuer"],
                audience: _config["Jwt:audience"],
                claims: claims,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
                expires: DateTime.UtcNow.AddHours(8)
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}