using Shared.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Room.Models
{
    public sealed class PlacedAssetChecklist
    {
        public Guid Id { get; private set; }
        public Guid PlacedAssetId { get; private set; }
        public string StudyYear { get; private set; } = null!;

        private readonly List<PlacedAssetChecklistEntry> _entries = [];
        public IReadOnlyList<PlacedAssetChecklistEntry> Entries => _entries;

        private PlacedAssetChecklist() { }

        public PlacedAssetChecklist(
            Guid placedAssetId,
            string studyYear,
            IEnumerable<ChecklistItemTemplate> templateItems)
        {
            if (placedAssetId == Guid.Empty)
                throw new ArgumentException("Required.", nameof(placedAssetId));
            ArgumentException.ThrowIfNullOrWhiteSpace(studyYear);

            var items = templateItems.ToList();
            if (items.Count == 0)
                throw new DomainException("CHECKLIST_EMPTY_TEMPLATE", "Empty template.");

            Id = Guid.NewGuid();
            PlacedAssetId = placedAssetId;
            StudyYear = studyYear.Trim();
            _entries.AddRange(items.Select(template => new PlacedAssetChecklistEntry(this, template)));
        }

        public void Check(Guid checklistItemId, Guid checkedByUserId)
        {
            var entry = _entries.FirstOrDefault(e => e.ChecklistItem.Id == checklistItemId)
                ?? throw new DomainException("CHECKLIST_ENTRY_NOT_FOUND", "Entry not found.");
            if (entry.IsChecked)
                throw new DomainException("CHECKLIST_ENTRY_ALREADY_CHECKED", "Already checked.");

            entry.Mark(checkedByUserId);
        }

        public bool IsComplete => _entries.All(e => e.IsChecked);
        public int CheckedCount => _entries.Count(e => e.IsChecked);
        public int TotalCount => _entries.Count;
    }
}
