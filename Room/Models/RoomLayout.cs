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

        public List<PlacedAssetEntry> PlacedAssets = new();

        public void ApplySnapShot(List<PlacedAssetEntry> incomingAssets, Guid userId)
        {
            if (!incomingAssets.Any(a => a.Id == Guid.Empty))
                throw new InvalidOperationException("All Assets Must Have Valid Id");

            if (!incomingAssets.Any(a => a.AssetDefinitionId == Guid.Empty))
                throw new InvalidOperationException("All Assets Must Have Valid Asset Id");

            var conditionMap = PlacedAssets.ToDictionary(pa => pa.Id, pa => pa.Condition);

            PlacedAssets = incomingAssets;
            foreach (var asset in incomingAssets)
            {
                asset.Condition = conditionMap.TryGetValue(asset.Id, out var condition)
                    ? condition
                    : Shared.Enums.PlacedAssetCondition.Good;

                LastModifiedDate = DateTime.UtcNow;
                LastModifiedUserId = userId;
            }
        }

        public void UpdateAssetCondition(Guid placedAssetId, PlacedAssetCondition condition)
        {
            var entry = PlacedAssets.FirstOrDefault(pa => pa.Id == placedAssetId);
            if (entry is not null) entry.Condition = condition;
        }
    }
}
