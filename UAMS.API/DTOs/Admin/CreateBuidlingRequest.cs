using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace UAMS.API.DTOs.Admin
{
    public sealed record CreateBuidlingRequest(string name, string address);
    public sealed record LinkFacultyToBuildingRequest(Guid buildingId, Guid facultyId);
    public sealed record CreateAssetManagerRequest(
        string email,
        string password,
        string fullName,
        Guid facultyId);

    public sealed record CreateDeptManagerRequest(
        string email,
        string password,
        string fullName,
        Guid departmentId);

    public sealed record AssignFacultyRequest(Guid facultyId);
}