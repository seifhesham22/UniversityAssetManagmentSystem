using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Campus.Models
{
    public sealed class TeacherFaculty
    {
        public Guid TeacherId { get; private set; }
        public Guid FacultyId { get; private set; }
        public Teacher Teacher { get; private set; } = null!;
        public Faculty Faculty { get; private set; } = null!;
        public DateTime AssignedAtUtc { get; private set; }

        private TeacherFaculty() { }

        public TeacherFaculty(Guid teacherId, Guid facultyId)
        {
            TeacherId = teacherId;
            FacultyId = facultyId;
            AssignedAtUtc = DateTime.UtcNow;
        }
    }
}