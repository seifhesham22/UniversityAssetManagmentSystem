using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared.Authorization;
using Shared.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UAMS.Identity.Facades;
using UAMS.Identity.IdentityModels;

namespace UAMS.Identity.Services.TokenService
{
    public class TokenHandler : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly IGetCampusUser _getCampusUser;
        public TokenHandler(
            IConfiguration config,
            IGetCampusUser getCampusUser)
        {
            _config = config;
            _getCampusUser = getCampusUser;
        }

        public async Task<string> GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var expireHours = double.Parse(_config["Jwt:ExpireHours"]);
            var expires = DateTime.UtcNow.AddHours(expireHours);
            var claims = new List<Claim>
            {
                new(AppClaims.UserId, user.Id.ToString()),
                new(AppClaims.Role, user.Role.ToString()),
            };

            switch (user.Role)
            {
                case Role.Student:
                    var facultyId = await _getCampusUser.GetStudentFacultyId(user.Id);
                    claims.Add(new(AppClaims.FacultyId, facultyId.ToString()));
                    break;

                case Role.DepartmentManager:
                    var departmentId = await _getCampusUser.GetDeptManagerDepartmentId(user.Id);
                    claims.Add(new(AppClaims.DepartmentId, departmentId.ToString()));
                    break;

                case Role.Maintainer:
                    var DepartmentId = await _getCampusUser.GetMaintainerDepartmentId(user.Id);
                    claims.Add(new(AppClaims.DepartmentId, DepartmentId.ToString()));
                    break;

                case Role.AssetManager:
                    var FacultyId = await _getCampusUser.GetAssetManagerFacultyId(user.Id);
                    claims.Add(new(AppClaims.FacultyId, FacultyId.ToString()));
                    break;
            }

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
                expires: expires
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}