using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace UAMS.API.DTOs.Auth
{
    public sealed record RegisterTeacherRequest(
        [Required]
        [EmailAddress]
        [NotNull]
        string email,
        [Required]
        string password,
        [Required]
        [NotNull]
        [MaxLength(50)]
        string fullName);
}