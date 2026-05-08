namespace UAMS.Room.ViewDtos
{
    public sealed record AssetDto(
        Guid AssetDefinitionId,
        string AssetName,
        string SvgUrl,
        List<string> AllowedLocations);
}
