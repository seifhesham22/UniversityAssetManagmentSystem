using Shared.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Campus.Models
{
    public sealed class Teacher
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string FullName { get; private set; } = null!;
        public bool CanDesign { get; private set; }

        private readonly List<TeacherFaculty> _faculties = new();
        public IReadOnlyList<TeacherFaculty> Faculties => _faculties.AsReadOnly();

        private Teacher() { }

        public Teacher(Guid userId, string fullName)
        {
            if (userId == Guid.Empty) throw new ArgumentException(nameof(userId));
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name required.", nameof(fullName));

            Id = Guid.NewGuid();
            UserId = userId;
            FullName = fullName.Trim();
            CanDesign = false;
        }

        public TeacherFaculty AssignToFaculty(Guid facultyId)
        {
            if (facultyId == Guid.Empty)
                throw new DomainException("TEACHER_FACULTY_INVALID", "Faculty id invalid.");
            if (_faculties.Any(f => f.FacultyId == facultyId))
                throw new DomainException("TEACHER_FACULTY_DUPLICATE", "Teacher already assigned to this faculty.");

            var entry = new TeacherFaculty(Id, facultyId);
            _faculties.Add(entry);
            return entry;
        }

        public void RemoveFromFaculty(Guid facultyId)
        {
            var entry = _faculties.FirstOrDefault(f => f.FacultyId == facultyId)
                ?? throw new DomainException("TEACHER_FACULTY_NOT_FOUND", "Teacher is not in this faculty.");
            _faculties.Remove(entry);
        }

        public bool BelongsToFaculty(Guid facultyId) =>
            _faculties.Any(f => f.FacultyId == facultyId);

        public void GrantDesignAccess()
        {
            if (CanDesign)
                throw new DomainException("TEACHER_DESIGN_ALREADY_GRANTED", "Design access already granted.");
            CanDesign = true;
        }

        public void RevokeDesignAccess()
        {
            if (!CanDesign)
                throw new DomainException("TEACHER_DESIGN_ALREADY_REVOKED", "Design access already revoked.");
            CanDesign = false;
        }
    }
}