using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Authorization
{
    public class CurrentUser
    {
        public Guid UserId { get; init; }
        public Role Role { get; init; }
        public string Email { get; init; } = string.Empty;
        public bool IsInRole(Role role) => Role == role;
    }
}