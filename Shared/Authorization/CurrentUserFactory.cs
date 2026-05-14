using Microsoft.AspNetCore.Http;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Shared.Authorization
{
    public class CurrentUserFactory
    {
        private readonly IHttpContextAccessor _http;
        public CurrentUserFactory(IHttpContextAccessor http) => _http = http;
        public CurrentUser Create()
        {
            var user = _http.HttpContext?.User
            ?? throw new UnauthorizedAccessException("No HTTP context.");
            var userId = user.FindFirstValue(AppClaims.UserId)
            ?? throw new UnauthorizedAccessException("UserId claim missing.");
            var role = user.FindFirstValue(AppClaims.Role);
            var facultyId = user.FindFirstValue(AppClaims.FacultyId);
            var departmentId = user.FindFirstValue(AppClaims.DepartmentId);
            return new CurrentUser
            {
                UserId = Guid.Parse(userId),
                Role = Enum.Parse<Role>(role),
                Email = user.FindFirstValue(AppClaims.Email) ?? string.Empty,
                facultyId = Guid.TryParse(facultyId, out var g) ? g : null,
                departmentId = Guid.TryParse(departmentId, out var d) ? d : null
            };
        }
    }
}