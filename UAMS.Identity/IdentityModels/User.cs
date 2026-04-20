using Microsoft.AspNetCore.Identity;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace UAMS.Identity.IdentityModels
{
    public class User : IdentityUser<Guid>
    {
        public Role Role { get; private set; }
        public bool IsActive { get; private set; } = true;

        private User() { }

        public User(string email, Role role)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email), $"Email can't be null or empty");
            Email = email;
            Role = role;
        }

        public void Deactivate() => IsActive = false;
        public void ReActivate() => IsActive = true;
    }
}