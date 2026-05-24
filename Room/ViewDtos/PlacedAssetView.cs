namespace UAMS.Room.ViewDtos
{
    public sealed record PlacedAssetView(
        Guid Id,
        Guid AssetDefinitionId,
        string AssetName,
        float X,
        float Y,
        float Width,
        float Height,
        float Rotation,
        string Condition,
        Guid? GroupId,
        string? GroupLabel,
        Guid? CanvasRoomId);
}
