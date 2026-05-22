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

        public DepartmentManager(Guid userId, string fullName, Guid departmentId)
        {
            if (userId == Guid.Empty) throw new ArgumentException(nameof(userId));
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name required.", nameof(fullName));

            Id = Guid.NewGuid();
            UserId = userId;
            FullName = fullName.Trim();
            DepartmentId = departmentId;
        }

        public void assignToDepartment(Guid departmentId, Department department)
        {
            this.DepartmentId = departmentId;
            this.Department = department;
        }

        public void ReassignToDepartment(Guid newDepartmentId)
        {
            if (newDepartmentId == Guid.Empty)
                throw new ArgumentException("Department id required.", nameof(newDepartmentId));
            if (newDepartmentId == DepartmentId)
                throw new InvalidOperationException("Manager is already assigned to this department.");
            DepartmentId = newDepartmentId;
        }
    }
}