namespace UAMS.Room.Models
{
    public sealed class PlacedAssetChecklistEntry
    {
        public Guid Id { get; private set; }
        public PlacedAssetChecklist AssetChecklist { get; private set; } = null!;
        public ChecklistItemTemplate ChecklistItem { get; private set; } = null!;

        public bool IsChecked { get; private set; }
        public Guid? CheckedByUserId { get; private set; }
        public DateTime? CheckedAtUtc { get; private set; }

        private PlacedAssetChecklistEntry() { }

        internal PlacedAssetChecklistEntry(PlacedAssetChecklist assetChecklist, ChecklistItemTemplate checklistItem)
        {
            Id = Guid.NewGuid();
            AssetChecklist = assetChecklist;
            ChecklistItem = checklistItem;
            IsChecked = false;
        }

        internal void Mark(Guid userId)
        {
            IsChecked = true;
            CheckedByUserId = userId;
            CheckedAtUtc = DateTime.UtcNow;
        }
    }
}