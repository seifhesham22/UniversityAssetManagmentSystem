using Microsoft.AspNetCore.Server.HttpSys;
using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Room.Models
{
    public sealed class ChecklistItemTemplate
    {
        public Guid Id { get; private set; }
        public Guid AssetId { get; private set; }
        public string Description { get; private set; } = null!;

        internal ChecklistItemTemplate(Guid assetId, string description)
        {
            if (string.IsNullOrEmpty(Description))
                throw new ArgumentNullException("description can't be empty");

            Id = Guid.NewGuid();
            AssetId = assetId;
            Description = description;
        }
    }
}