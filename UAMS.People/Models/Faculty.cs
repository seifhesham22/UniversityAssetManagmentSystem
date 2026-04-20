using Shared.Abstractions;

namespace UAMS.Campus.Models
{
    public sealed class Faculty
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public bool IsActive { get; private set; }

        private readonly List<Student> _students = new();
        public IReadOnlyCollection<Student> Students => _students;

        private readonly List<AssetManager> _assetManagers = new();
        public IReadOnlyCollection<AssetManager> AssetManagers => _assetManagers;

        private readonly List<FacultyBuilding> _buildingLinks = new();
        public IReadOnlyList<FacultyBuilding> BuildingLinks => _buildingLinks.AsReadOnly();

        private Faculty() { }

        public Faculty(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
            Id = Guid.NewGuid();
            Name = name.Trim();
            IsActive = true;
        }

        public FacultyBuilding LinkToBuilding(Guid buildingId)
        {
            if (_buildingLinks.Any(fb => fb.BuildingId == buildingId))
                throw new DomainException("FACULTY_BUILDING_DUPLICATE", "Building already linked.");
            var link = new FacultyBuilding(Id, buildingId);
            _buildingLinks.Add(link);
            return link;
        }

        public void UnlinkBuilding(Guid buildingId)
        {
            var link = _buildingLinks.FirstOrDefault(fb => fb.BuildingId == buildingId)
                ?? throw new DomainException("FACULTY_BUILDING_NOT_FOUND", "Building not linked.");
            _buildingLinks.Remove(link);
        }

        public void Archive() => IsActive = false;
        public void Restore() => IsActive = true;
    }
}