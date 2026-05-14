using System;
using System.Collections.Generic;
using UAMS.Identity.IdentityModels;

namespace UAMS.Identity.Services.TokenService
{
    public interface ITokenService
    {
        public Task<string> GenerateToken(User user);
    }
}