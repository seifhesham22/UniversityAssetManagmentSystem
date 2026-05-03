using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace UAMS.API.DTOs.Auth
{
    public sealed record LoginRequestForAll(
        [Required]
        [EmailAddress]
        [NotNull]
        string email,
        [NotNull]
        [Required]
        string password);
}