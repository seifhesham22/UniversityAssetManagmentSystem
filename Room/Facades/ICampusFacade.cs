namespace UAMS.Room.Facades
{
    public interface ICampusFacade
    {
        Task<Dictionary<Guid, string>> GetUserNamesAsync(IEnumerable<Guid> userIds, CancellationToken ct);
        Task<string?> GetFacultyNameAsync(Guid facultyId, CancellationToken ct);
        Task<string?> GetBuildingNameAsync(Guid buildingId, CancellationToken ct);
        Task<string?> GetDepartmentNameAsync(Guid departmentId, CancellationToken ct);
        Task<string?> GetMaintainerNameAsync(Guid maintainerId, CancellationToken ct);
        Task<Guid?> GetMaintainerIdByUserIdAsync(Guid userId, CancellationToken ct);
        Task<Guid?> GetDepartmentManagerDepartmentIdAsync(Guid userId, CancellationToken ct);
        Task<bool> IsMaintainerInDepartmentAsync(Guid maintainerId, Guid departmentId, CancellationToken ct);
        Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken ct);
        Task<string?> GetMaintainerVkIdAsync(Guid maintainerId, CancellationToken ct);
        Task<Guid?> GetMaintainerIdByVkIdAsync(string vkId, CancellationToken ct);
        Task<string?> GetAssetManagerNameByFacultyIdAsync(Guid facultyId, CancellationToken ct);
        Task<(string Name, string? Address)?> GetBuildingInfoAsync(Guid buildingId, CancellationToken ct);
        Task<Dictionary<Guid, string>> GetNoteAuthorNamesAsync(IEnumerable<Guid> authorIds, CancellationToken ct);
        Task<Dictionary<Guid, (string Name, string Role)>> GetNoteAuthorInfoAsync(IEnumerable<Guid> authorIds, CancellationToken ct);
    }
}
