namespace UAMS.Room.Models
{
    public sealed class CompositeTemplate
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public List<CompositeTemplateItemData> Items { get; private set; } = new();

        private CompositeTemplate() { }

        public CompositeTemplate(string name, List<CompositeTemplateItemData> items)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.");
            if (items == null || items.Count == 0) throw new ArgumentException("At least one item required.");
            Id = Guid.NewGuid();
            Name = name.Trim();
            Items = items;
        }
    }

    public sealed class CompositeTemplateItemData
    {
        public Guid AssetDefinitionId { get; set; }
        public float RelX { get; set; }
        public float RelY { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Rotation { get; set; }
    }
}
