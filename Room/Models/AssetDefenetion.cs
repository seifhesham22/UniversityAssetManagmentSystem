using Shared.Abstractions;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Room.Models
{
    public sealed class AssetDefenetion
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;

        public AssetCategory Category { get; private set; }
        public string SvgUrl { get; private set; } = null!;
        private readonly List<PlacementLocation> _allowedLocations = new();
        public IReadOnlyCollection<PlacementLocation> AllowedLocations => _allowedLocations;

        private readonly List<ChecklistItemTemplate> _checklistTemplate = new();
        public IReadOnlyCollection<ChecklistItemTemplate> ChecklistTemplate => _checklistTemplate;

        private AssetDefenetion() { }

        public AssetDefenetion(
            string name,
            AssetCategory category,
            IEnumerable<PlacementLocation> placementLocations,
            string svgUrl)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name can't be null or empty");
            if (string.IsNullOrEmpty(svgUrl)) throw new ArgumentNullException("name can't be null or empty");

            var locations = placementLocations.Distinct().ToList();
            if (locations.Count == 0)
                throw new InvalidOperationException("At least one location must be there");

            Id = Guid.NewGuid();
            Name = name.Trim();
            Category = category;
            _allowedLocations.AddRange(locations);
            SvgUrl = svgUrl;
        }

        public ChecklistItemTemplate AddChecklistItem(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException("Description required.");

            if (_checklistTemplate.Any(i =>
                    string.Equals(i.Description, description.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new DomainException("CHECKLIST_ITEM_DUPLICATE", "Duplicate description.");

            var item = new ChecklistItemTemplate(Id, description.Trim());
            _checklistTemplate.Add(item);
            return item;
        }

        public void RemoveChecklistItem(Guid itemId)
        {
            var item = _checklistTemplate.FirstOrDefault(i => i.Id == itemId)
                ?? throw new ArgumentNullException("Item not found.");
            _checklistTemplate.Remove(item);
        }

        public void Update(
            string name,
            string svgUrl,
            AssetCategory category,
            IEnumerable<PlacementLocation> locations)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
            if (string.IsNullOrWhiteSpace(svgUrl)) throw new ArgumentException("SvgUrl required.", nameof(svgUrl));

            var locs = locations.Distinct().ToList();
            if (locs.Count == 0)
                throw new InvalidOperationException("At least one placement location is required.");

            Name = name.Trim();
            SvgUrl = svgUrl.Trim();
            Category = category;
            _allowedLocations.Clear();
            _allowedLocations.AddRange(locs);
        }
    }
}