using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Identity.Facades
{
    public interface IGetCampusUser
    {
        public Task<Guid?> GetStudentFacultyId(Guid userId);
        public Task<Guid?> GetAssetManagerFacultyId(Guid userId);
        public Task<Guid?> GetMaintainerDepartmentId(Guid userId);
        public Task<Guid?> GetDeptManagerDepartmentId(Guid userId);
    }
}
