namespace UAMS.Room.ViewDtos
{
    public sealed record AssetDefinitionListItem(
        Guid Id,
        string Name,
        string Category,
        string SvgUrl,
        List<string> AllowedLocations);
}
