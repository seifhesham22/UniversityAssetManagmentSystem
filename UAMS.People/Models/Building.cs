using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Campus.Models
{
    public sealed class Building
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public string Address { get; private set; } = null!;

        private readonly List<FacultyBuilding> _facultyLinks = new();
        public IReadOnlyList<FacultyBuilding> FacultyLinks => _facultyLinks.AsReadOnly();

        private Building() { }

        public Building(string name, string address)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
            if (string.IsNullOrWhiteSpace(address)) throw new ArgumentException(nameof(address));

            Id = Guid.NewGuid();
            Name = name.Trim();
            Address = address.Trim();
        }

        public void Rename(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName)) throw new ArgumentException(nameof(newName));
            Name = newName.Trim();
        }
    }
}