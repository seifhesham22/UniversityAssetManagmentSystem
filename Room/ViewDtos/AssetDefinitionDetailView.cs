namespace UAMS.Room.ViewDtos
{
    public sealed record AssetDefinitionDetailView(
        Guid Id,
        string Name,
        string Category,
        string SvgUrl,
        List<string> AllowedLocations,
        List<ChecklistItemView> ChecklistItems);
}
