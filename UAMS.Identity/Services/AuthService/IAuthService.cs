using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using UAMS.Identity.IdentityModels;
using UAMS.Identity.Services.TokenService;

namespace UAMS.Identity.Services.AuthService
{
    public interface IAuthService
    {
        /// <summary>
        /// Method For Loging in the user in the system.
        /// </summary>
        /// <param name="email">Represents the user Email</param>
        /// <param name="password">Represents the User password</param>
        /// <param name="ct">Represents the CancellationToken</param>
        /// <returns>Returns Jwt Token and its expiry date</returns>
        Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct);
        Task<Guid> CreateUserAsync(
            string email,
            string password,
            Role role,
            CancellationToken ct);
        Task DeleteUserAsync(Guid userId, CancellationToken ct);
        Task ActivateAsync(Guid userId, CancellationToken ct);
        Task DeActivateAsync(Guid userId, CancellationToken ct);
    }
    public sealed record AuthResult(string Token, string Email, string Role);
    public class AuthService(
        UserManager<User> _userManager,
        ITokenService _token) 
        : IAuthService
    {
        public async Task ActivateAsync(Guid userId, CancellationToken ct)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString()) 
                ?? throw new InvalidOperationException($"Couldn't find a user with the Id {userId}");

            if (user.IsActive)
                throw new InvalidOperationException("user is already active");

            user.Deactivate();

            var res = await _userManager.UpdateAsync(user);

            if (!res.Succeeded)
            {
                var errs = string.Join(", ", res.Errors.SelectMany(x => x.Description).ToList());
                throw new InvalidOperationException(errs);
            }
        }

        public async Task<Guid> CreateUserAsync(string email, string password, Role role, CancellationToken ct)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is not null)
                throw new InvalidOperationException("User already exists under this email");

            var User = new User(email, role);
            User.UserName = email;

            var res = await _userManager.CreateAsync(User, password);
            if (!res.Succeeded)
            {
                var errs = string.Join(", ", res.Errors.Select(x => x.Description).ToList());
                throw new InvalidOperationException(errs);
            }

            return User.Id;
        }

        public async Task DeActivateAsync(Guid userId, CancellationToken ct)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
               ?? throw new InvalidOperationException($"Couldn't find a user with the Id {userId}");

            if (!user.IsActive)
                throw new InvalidOperationException("user is already deactivated");

            user.Deactivate();

            var res = await _userManager.UpdateAsync(user);

            if (!res.Succeeded)
            {
                var errs = string.Join(", ", res.Errors.SelectMany(x => x.Description).ToList());
                throw new InvalidOperationException(errs);
            }
        }

        public async Task DeleteUserAsync(Guid userId, CancellationToken ct)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new InvalidOperationException($"couldn't find user with the id {userId}");

            var res = await _userManager.DeleteAsync(user);
            if (!res.Succeeded)
            {
                var errs = string.Join(", ", res.Errors.SelectMany(x => x.Description).ToList());
                throw new InvalidOperationException(errs);
            }
        }

        public async Task<AuthResult> LoginAsync(string email, string password, CancellationToken ct)
        {
            var user = await _userManager.FindByNameAsync(email)
                ?? throw new InvalidOperationException("Invalid credentials");

            var res = await _userManager.CheckPasswordAsync(user, password);
            if (!res)
                throw new InvalidOperationException("Invalid email or pass");

            var token = await _token.GenerateToken(user);
            return new AuthResult(
                Token: token,
                Email: user.Email!,
                Role: user.Role.ToString()
                );
        }
    }
}