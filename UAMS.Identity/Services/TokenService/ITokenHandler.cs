using System;
using System.Collections.Generic;
using UAMS.Identity.IdentityModels;

namespace UAMS.Identity.Services.TokenService
{
    public interface ITokenService
    {
        public string GenerateToken(User user);
    }
}