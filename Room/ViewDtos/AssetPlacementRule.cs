namespace UAMS.Room.ViewDtos
{
    public sealed record AssetPlacementRule(
        Guid AssetDefinitionId,
        string AssetName,
        string Category,
        List<string> AllowedLocations);
}
