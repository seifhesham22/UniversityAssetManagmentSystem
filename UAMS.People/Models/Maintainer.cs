using Shared.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Campus.Models
{
    public sealed class Maintainer
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string FullName { get; private set; } = null!;
        public Guid? DepartmentId { get; private set; }
        public Department? Department { get; private set; }
        public bool IsActive { get; private set; }

        private Maintainer() { }

        public Maintainer(Guid userId, string fullName, Guid departmentId)
        {
            if (userId == Guid.Empty) throw new ArgumentException(nameof(userId));
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name required.", nameof(fullName));

            Id = Guid.NewGuid();
            UserId = userId;
            FullName = fullName.Trim();
            IsActive = true;
            DepartmentId = departmentId;
        }

        public void AssignToDepartment(Guid departmentId)
        {
            if (!IsActive)
                throw new DomainException("MAINTAINER_INACTIVE", "Cannot assign an inactive maintainer.");
            if (DepartmentId.HasValue)
                throw new DomainException("MAINTAINER_ALREADY_ASSIGNED", "Maintainer already assigned to a department.");
            if (departmentId == Guid.Empty)
                throw new DomainException("MAINTAINER_DEPARTMENT_INVALID", "Department id invalid.");
            DepartmentId = departmentId;
        }

        public void RemoveFromDepartment()
        {
            if (!DepartmentId.HasValue)
                throw new DomainException("MAINTAINER_NOT_ASSIGNED", "Maintainer has no department.");
            DepartmentId = null;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}