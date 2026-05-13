using Shared.Abstractions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace UAMS.Campus.Models
{
    public sealed class DepartmentManager
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid DepartmentId { get; private set; }
        public Department Department { get; private set; } = null!;
        public string FullName { get; private set; } = null!;

        private DepartmentManager() { }

        public DepartmentManager(Guid userId, string fullName)
        {
            if (userId == Guid.Empty) throw new ArgumentException(nameof(userId));
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name required.", nameof(fullName));

            Id = Guid.NewGuid();
            UserId = userId;
            FullName = fullName.Trim();
        }

        public void assignToDepartment(Guid departmentId)
        {
            this.DepartmentId = departmentId;
        }
    }
}