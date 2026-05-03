namespace UAMS.Room.ViewDtos
{
    public sealed record LayoutView(
        List<PlacedAssetView> PlacedAssets,
        DateTime? LastModifiedAtUtc,
        Guid? LastModifiedByUserId);
}
