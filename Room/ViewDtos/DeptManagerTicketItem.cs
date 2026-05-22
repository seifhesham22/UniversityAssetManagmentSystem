namespace UAMS.Room.ViewDtos
{
    public sealed record DeptManagerTicketItem(
        Guid Id,
        string AssetName,
        Guid RoomId,
        string RoomName,
        Guid FacultyId,
        string FacultyName,
        string BuildingName,
        string Status,
        string Decision,
        Guid? CurrentMaintainerId,
        string? CurrentMaintainerName,
        DateTime CreatedAtUtc,
        DateTime UpdatedAtUtc,
        string VkNotificationStatus,
        IReadOnlyList<TicketNoteView> Notes);
}
