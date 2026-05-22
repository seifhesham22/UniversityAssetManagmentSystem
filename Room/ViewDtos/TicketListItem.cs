namespace UAMS.Room.ViewDtos
{
    public sealed record TicketListItem(
        Guid Id,
        string AssetName,
        string RoomName,
        string RoomCode,
        Guid FacultyId,
        string ReportedByName,
        string Status,
        string Decision,
        Guid? AssignedToDepartmentId,
        string? DepartmentName,
        Guid? CurrentMaintainerId,
        string? CurrentMaintainerName,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc,
        IReadOnlyList<TicketNoteView> Notes);
}
