using Microsoft.AspNetCore.Components.Forms;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Room.Models
{
    public sealed class PlacedAssetEntry
    {
        public Guid Id { get; set; }
        public Guid AssetDefinitionId { get; set; }
        public Guid? GroupId { get; set; }
        public string? GroupLabel { get; set; }
        public string AssetName { get; set; } = null!;
        public PlacedAssetCondition Condition { get; set; }

        //Locations On Canvas
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Rotation { get; set; } = 0;

        // The Id of the Infrastructure (room) asset this asset lives inside on the canvas.
        // Used so moving a room moves all its child assets together.
        public Guid? CanvasRoomId { get; set; }

        // Links this asset to a locked composite group on the canvas.
        public Guid? CompositeId { get; set; }

        // Free-form JSON for structural assets (e.g. the room polygon: vertices + wall openings).
        // Null for ordinary assets. Stored inside the Layout JSON column, so no migration is needed.
        public string? Metadata { get; set; }
    }
}