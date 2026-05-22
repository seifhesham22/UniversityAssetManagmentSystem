using Shared.Abstractions;
using Shared.Enums;

namespace UAMS.Campus.Models
{
    public sealed class Department
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public AssetCategory Handles { get; private set; }
        public Guid? DepartmentManagerId { get; private set; }
        public DepartmentManager? Manager { get; private set; } = null!;

        private readonly List<Maintainer> _maintainers = new();
        public IReadOnlyCollection<Maintainer> Maintainers => _maintainers;
        public bool IsActive { get; private set; }

        private Department() { }

        public Department(string name, AssetCategory handles)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));

            Id = Guid.NewGuid();
            Name = name.Trim();
            Handles = handles;
            IsActive = true;
        }

        public void ChangeManager(Guid newManagerId)
        {
            if (newManagerId == Guid.Empty)
                throw new DomainException("DEPT_MANAGER_INVALID", "Manager id invalid.");
            if (newManagerId == DepartmentManagerId)
                throw new DomainException("DEPT_MANAGER_UNCHANGED", "Already the manager.");
            DepartmentManagerId = newManagerId;
        }

        public void ClearManager() => DepartmentManagerId = null;

        public void Archive() => IsActive = false;
        public void Restore() => IsActive = true;
    }
}