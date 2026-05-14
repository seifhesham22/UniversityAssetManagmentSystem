using Shared.Abstractions;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Room.Models
{
    public sealed class RoomLayout
    {
        public int Version { get; set; } = 1;
        public Guid LastModifiedUserId { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public Guid RoomId { get; set; }

        public List<PlacedAssetEntry> PlacedAssets { get; private set; } = new();

        public LayoutDiffResult ApplySnapshot(List<PlacedAssetEntry> incoming, Guid userId)
        {
            if (incoming.Any(a => a.Id == Guid.Empty))
                throw new DomainException("PLACED_ASSET_INVALID_ID",
                    "All placed assets must have a valid Id.");
            if (incoming.Any(a => a.AssetDefinitionId == Guid.Empty))
                throw new DomainException("PLACED_ASSET_INVALID_DEF",
                    "All placed assets must have a valid AssetDefinitionId.");

            var currentById = PlacedAssets.ToDictionary(pa => pa.Id);
            var incomingIds = incoming.Select(a => a.Id).ToHashSet();

            // Assets that existed before but are gone in the new snapshot.
            var removedIds = currentById.Keys.Where(id => !incomingIds.Contains(id)).ToList();

            // New assets not previously in the layout.
            var addedIds = incomingIds.Where(id => !currentById.ContainsKey(id)).ToHashSet();

            // Apply: preserve Condition on existing, default Good on new.
            foreach (var asset in incoming)
            {
                if (currentById.TryGetValue(asset.Id, out var existing))
                    asset.Condition = existing.Condition;
                else
                    asset.Condition = PlacedAssetCondition.Good;
            }

            PlacedAssets = incoming;
            LastModifiedDate = DateTime.UtcNow;
            LastModifiedUserId = userId;

            return new LayoutDiffResult(removedIds, addedIds);
        }

        public void UpdateAssetCondition(Guid placedAssetId, PlacedAssetCondition condition)
        {
            var entry = PlacedAssets.FirstOrDefault(pa => pa.Id == placedAssetId);
            if (entry is not null) entry.Condition = condition;
        }

        public bool HasAsset(Guid placedAssetId) =>
            PlacedAssets.Any(pa => pa.Id == placedAssetId);

        public PlacedAssetEntry? FindAsset(Guid placedAssetId) =>
            PlacedAssets.FirstOrDefault(pa => pa.Id == placedAssetId);
    }

    public sealed record LayoutDiffResult(
        List<Guid> RemovedPlacedAssetIds,
        HashSet<Guid> AddedPlacedAssetIds);
    }
