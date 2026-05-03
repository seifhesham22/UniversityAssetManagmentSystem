namespace UAMS.Room.ViewDtos
{
    public sealed record TicketDetailView(
        Guid Id,
        Guid PlacedAssetId,
        string AssetName,
        Guid RoomId,
        string RoomName,
        string RoomCode,
        Guid FacultyId,
        Guid ReportedByUserId,
        string ReportedByName,
        string Status,
        string Decision,
        Guid? AssignedToDepartmentId,
        string? DepartmentName,
        Guid? CurrentMaintainerId,
        string? CurrentMaintainerName,
        Guid? ConfirmedByUserId,
        string? ConfirmedByName,
        DateTime CreatedAtUtc, DateTime UpdatedAtUtc,
        List<TicketNoteView> Notes);
}