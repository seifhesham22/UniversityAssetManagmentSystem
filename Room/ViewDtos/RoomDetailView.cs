namespace UAMS.Room.ViewDtos
{
    public sealed record RoomDetailView(
        Guid Id,
        string Name,
        Guid BuildingId,
        Guid FacultyId,
        string Status,
        string? ClosureReason,
        Guid DesignedByUserId,
        LayoutView Layout);
}
