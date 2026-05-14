using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using UAMS.Campus.Presistence;
using UAMS.Identity.Facades;

namespace UAMS.Campus.FacadeImplementations
{
    public sealed class GetCampusUser(CampusDbContext _db) : IGetCampusUser
    {
        public async Task<Guid?> GetAssetManagerFacultyId(Guid userId)
        {
            var assetManager = await _db
                .asset_managers
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if(assetManager is null) return null;

            return assetManager.FacultyId;
        }

        public async Task<Guid?> GetDeptManagerDepartmentId(Guid userId)
        {
            var deptManager = await _db
               .department_managers
               .FirstOrDefaultAsync(x => x.UserId == userId);

            if(deptManager is null) return null;

            return deptManager.DepartmentId;
        }

        public async Task<Guid?> GetMaintainerDepartmentId(Guid userId)
        {
            var maintainer = await _db
                .maintainers
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if(maintainer is null) return null;

            return maintainer.DepartmentId;
        }

        public async Task<Guid?> GetStudentFacultyId(Guid userId)
        {
            var student = await _db
                .students
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if(student is null) return null;

            return student.FacultyId;
        }
    }
}