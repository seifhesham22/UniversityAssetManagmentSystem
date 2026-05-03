namespace UAMS.Room.ViewDtos
{
    public sealed record RoomDetailView(
        Guid Id,
        string Name,
        string Code,
        Guid BuildingId,
        Guid FacultyId,
        string Status,
        string? ClosureReason,
        Guid DesignedByUserId,
        LayoutView Layout);
}
