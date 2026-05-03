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
        public Guid AssetId { get; set; }
        public Guid? GroupId { get; set; }
        public string? GroupLabel { get; set; }
        public string AssetName { get; set; } = null!;
        public PlacedAssetCondition Condition { get; set; }

        //Locations On Canvas
        public float X { get; set; }
        public float Y { get; set; }
        // Size for the Resizable SVGS 
        public float Width { get; set; }
        public float Height { get; set; }
    }
}