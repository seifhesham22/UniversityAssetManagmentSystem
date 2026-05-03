namespace UAMS.Room.ViewDtos
{
    public sealed record ChecklistEntryView(
        Guid Id,
        Guid ChecklistItemId,
        string Description,
        bool IsChecked,
        Guid? CheckedByUserId,
        DateTime? CheckedAtUtc);
}
