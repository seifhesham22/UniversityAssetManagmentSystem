using MediatR;
using Microsoft.EntityFrameworkCore;
using UAMS.Room.Models;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.LayoutFeatures
{
    public sealed record SavedPlacedAssetDto(
    Guid Id,
    Guid AssetDefinitionId,
    string AssetName,
    float X,
    float Y,
    Guid? GroupId,
    string? GroupLabel);
    public sealed record SaveLayoutCommand(
        Guid UserId,
        Guid RoomId,
        List<SavedPlacedAssetDto> PlacedAssets) : IRequest;
    public sealed class SaveLayoutCommandHandler(RoomDesignDbContext _db)
        : IRequestHandler<SaveLayoutCommand>
    {
        public async Task Handle(SaveLayoutCommand request, CancellationToken cancellationToken)
{
            var room = await _db.Rooms.FirstOrDefaultAsync(room => room.Id == request.RoomId)
                ?? throw new InvalidOperationException($"Couldn't find a room with the Id {request.RoomId}");

            var incomingAssets = request.PlacedAssets.Select(dto => new PlacedAssetEntry
            {
                Id = dto.Id,
                AssetName = dto.AssetName,
                AssetDefinitionId = dto.AssetDefinitionId,
                GroupId = dto.GroupId,
                GroupLabel = dto.GroupLabel 
            }).ToList();

            room.Layout.ApplySnapShot(incomingAssets, request.UserId);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}