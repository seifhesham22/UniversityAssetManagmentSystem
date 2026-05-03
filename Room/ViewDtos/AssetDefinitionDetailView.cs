namespace UAMS.Room.ViewDtos
{
    public sealed record AssetDefinitionDetailView(
        Guid Id,
        string Name,
        string Category,
        List<string> AllowedLocations,
        List<ChecklistItemView> ChecklistItems);
}
