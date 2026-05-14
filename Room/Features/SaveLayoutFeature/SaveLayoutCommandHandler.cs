using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using System.Collections.Concurrent;
using UAMS.Room.Models;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.LayoutFeatures
{
    public sealed record SaveLayoutCommand(
     Guid RoomId,
     Guid UserId,
     List<PlacedAssetDto> PlacedAssets) : IRequest;

    public sealed record PlacedAssetDto(
        Guid Id,
        Guid AssetDefinitionId,
        string AssetName,
        float X, float Y,
        float W, float H,
        Guid? GroupId,
        string? GroupLabel);

    internal sealed class SaveLayoutHandler(RoomDesignDbContext db)
        : IRequestHandler<SaveLayoutCommand>
    {
        public async Task Handle(SaveLayoutCommand cmd, CancellationToken ct)
        {
            var room = await db.Rooms
                .FirstOrDefaultAsync(r => r.Id == cmd.RoomId, ct)
                ?? throw new DomainException("ROOM_NOT_FOUND", "Room not found.");

            var requestedDefIds = cmd.PlacedAssets
                .Select(a => a.AssetDefinitionId)
                .Distinct()
                .ToList();

            var existingDefIds = await db.AssetDefinitions
                .Where(ad => requestedDefIds.Contains(ad.Id))
                .Select(ad => ad.Id)
                .ToHashSetAsync(ct);

            var missing = requestedDefIds.Where(id => !existingDefIds.Contains(id)).ToList();
            if (missing.Count > 0)
                throw new DomainException("ASSET_DEF_NOT_FOUND",
                    $"Unknown AssetDefinitionIds: {string.Join(", ", missing)}");

            var incoming = cmd.PlacedAssets.Select(a => new PlacedAssetEntry
            {
                Id = a.Id,
                AssetDefinitionId = a.AssetDefinitionId,
                AssetName = a.AssetName,
                X = a.X,
                Y = a.Y,
                Width = a.W,
                Height = a.H,
                GroupId = a.GroupId,
                GroupLabel = a.GroupLabel,
            }).ToList();

            var diff = room.Layout.ApplySnapshot(incoming, cmd.UserId);

            if (diff.RemovedPlacedAssetIds.Count > 0)
            {
                var checklistsToRemove = await db.PlacedAssetCheckLists
                    .Where(c => diff.RemovedPlacedAssetIds.Contains(c.PlacedAssetId))
                    .ToListAsync(ct);

                db.PlacedAssetCheckLists.RemoveRange(checklistsToRemove);
            }

            await db.SaveChangesAsync(ct);
        }
    }
}