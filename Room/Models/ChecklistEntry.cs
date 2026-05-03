namespace UAMS.Room.Models
{
    public sealed class ChecklistEntry
    {
        public Guid Id { get; private set; }
        public Guid AssetChecklistId { get; private set; }
        public Guid ChecklistItemId { get; private set; }
        public bool IsChecked { get; private set; }
        public Guid? CheckedByUserId { get; private set; }
        public DateTime? CheckedAtUtc { get; private set; }

        private ChecklistEntry() { }

        internal ChecklistEntry(Guid assetChecklistId, Guid checklistItemId)
        {
            Id = Guid.NewGuid();
            AssetChecklistId = assetChecklistId;
            ChecklistItemId = checklistItemId;
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
