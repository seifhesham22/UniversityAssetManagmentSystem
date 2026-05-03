using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace UAMS.API.DTOs.Auth
{
    public sealed record RegisterStudentRequest(
        [Required]
        [NotNull]
        [EmailAddress]
        string email,
        [Required]
        string password,
        [Required]
        [NotNull]
        [MaxLength(50)]
        string fullName,
        [NotNull]
        [Required]
        Guid facultyId);
}