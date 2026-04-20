using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Shared.Authorization
{
    public static class AppClaims
    {
        public const string UserId = ClaimTypes.NameIdentifier;
        public const string Email = ClaimTypes.Email;
        public const string Role = ClaimTypes.Role;
    }
}
