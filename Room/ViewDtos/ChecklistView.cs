namespace UAMS.Room.ViewDtos
{
    public sealed record ChecklistView(
    Guid Id,
    Guid PlacedAssetId,
    string StudyYear,
    int CheckedCount,
    int TotalCount, List<ChecklistEntryView> Entries);
}
