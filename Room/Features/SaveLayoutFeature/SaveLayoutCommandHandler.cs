using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
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
    float Width,
    float Height,
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
            var room = await _db.Rooms
                .Include(room => room.Layout)
                .ThenInclude(x => x.PlacedAssets)
                .FirstOrDefaultAsync(room => room.Id == request.RoomId)
                ?? throw new ArgumentNullException($"couldn't find a room with the Id: {request.RoomId}");

            var currentIds = room.Layout.PlacedAssets.Select(x => x.Id).ToHashSet();
            var incomingIds = request.PlacedAssets.Select(x => x.Id).ToHashSet();

            var removedIds = currentIds.Except(incomingIds).ToList();
            var addedAssets = request.PlacedAssets.Where(x => !currentIds.Contains(x.Id)).ToList();

            var existingAssetsToUpdate = request.PlacedAssets.Where(x => currentIds.Contains(x.Id)).ToList();

            if (removedIds.Count > 0)
            {
                await _db.PlacedAssetCheckLists
                    .Where(c => removedIds.Contains(c.PlacedAssetId))
                    .ExecuteDeleteAsync(cancellationToken);

                var assetsToRemove = room.Layout.PlacedAssets.Where(x => removedIds.Contains(x.Id)).ToList();
                foreach (var item in assetsToRemove)
                {
                    room.Layout.PlacedAssets.Remove(item);
                }
            }

            if (existingAssetsToUpdate.Count > 0)
            {
                var trackedAssetsDict = room.Layout.PlacedAssets.ToDictionary(x => x.Id);

                foreach (var incoming in existingAssetsToUpdate)
                {
                    if (trackedAssetsDict.TryGetValue(incoming.Id, out var existingTrackedAsset))
                    {
                        existingTrackedAsset.X = incoming.X;
                        existingTrackedAsset.Y = incoming.Y;
                        existingTrackedAsset.Width = incoming.Width;
                        existingTrackedAsset.Height = incoming.Height;
                        existingTrackedAsset.GroupId = incoming.GroupId;
                        existingTrackedAsset.GroupLabel = incoming.GroupLabel;
                        existingTrackedAsset.AssetName = incoming.AssetName;
                    }
                }
            }

            if (addedAssets.Count > 0)
            {
                var addedDefinitionIds = addedAssets.Select(x => x.AssetDefinitionId).Distinct().ToList();

                var definitionDict = await _db.AssetDefinitions
                    .Include(x => x.ChecklistTemplate)
                    .Where(x => addedDefinitionIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, cancellationToken);

                var newChecklists = new List<PlacedAssetChecklist>(addedAssets.Count);
                var currentYear = DateTime.UtcNow.Year.ToString();

                foreach (var asset in addedAssets)
                {
                    room.Layout.PlacedAssets.Add(new PlacedAssetEntry
                    {
                        Id = asset.Id,
                        AssetDefinitionId = asset.AssetDefinitionId,
                        GroupId = asset.GroupId,
                        GroupLabel = asset.GroupLabel,
                        AssetName = asset.AssetName,
                        X = asset.X,
                        Y = asset.Y,
                        Width = asset.Width,
                        Height = asset.Height
                    });

                    if (definitionDict.TryGetValue(asset.AssetDefinitionId, out var definition)
                        && definition.ChecklistTemplate.Count > 0)
                    {
                        newChecklists.Add(new PlacedAssetChecklist(
                            asset.Id,
                            currentYear,
                            definition.ChecklistTemplate));
                    }
                }

                if (newChecklists.Count > 0)
                {
                    _db.PlacedAssetCheckLists.AddRange(newChecklists);
                }
            }
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}