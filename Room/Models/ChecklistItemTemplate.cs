using Microsoft.AspNetCore.Server.HttpSys;
using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Room.Models
{
    public sealed class ChecklistItemTemplate
    {
        public Guid Id { get; private set; }
        public Guid AssetDefinitionId { get; private set; }
        public AssetDefenetion AssetDefenetion { get; private set; } = null!;
        public string Description { get; private set; } = null!;

        private ChecklistItemTemplate() { }
        internal ChecklistItemTemplate(Guid assetId, string description)
        {
            if (string.IsNullOrEmpty(Description))
                throw new ArgumentNullException("description can't be empty");

            Id = Guid.NewGuid();
            AssetDefinitionId = assetId;
            Description = description;
        }
    }
}