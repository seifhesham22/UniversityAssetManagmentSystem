using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Campus.Models
{
    public sealed class AssetManager
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid FacultyId { get; private set; }
        public Faculty Faculty { get; private set; } = null!;
        public string FullName { get; private set; } = null!;

        private AssetManager() { }

        public AssetManager(Guid userId, Guid facultyId, string fullName)
        {
            if (userId == Guid.Empty) throw new ArgumentException(nameof(userId));
            if (facultyId == Guid.Empty) throw new ArgumentException(nameof(facultyId));
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name required.", nameof(fullName));

            Id = Guid.NewGuid();
            UserId = userId;
            FacultyId = facultyId;
            FullName = fullName.Trim();
        }
    }
}