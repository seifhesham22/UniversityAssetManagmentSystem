using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;

namespace UAMS.API.DTOs.Admin
{
    public sealed record CreateBuidlingRequest(string name, string address);
    public sealed record LinkFacultyToBuildingRequest(Guid buildingId, Guid facultyId);
    public sealed record UnLinkFacultyToBuildingRequest(Guid buildingId, Guid facultyId);
    public sealed record CreateAssetManagerRequest(
        string email,
        string password,
        string fullName,
        Guid facultyId);

    public sealed record CreateMaintianerRequest(
        string email,
        string password,
        string fullName,
        string? vkId = null);

    public sealed record CreateDeptManagerRequest(
        string email,
        string password,
        string fullName,
        Guid departmentId);

    public sealed record CreateRoomRequest(Guid facultyId, Guid buildingId, [Required] string name);
}