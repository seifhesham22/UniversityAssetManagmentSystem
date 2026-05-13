using Microsoft.EntityFrameworkCore;
using Shared.Authorization;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;
using UAMS.Room.Facades;

namespace UAMS.Campus.FacadeImplementations
{
    public sealed class FacultyFacade(
        CampusDbContext _db,
        CurrentUserFactory _currentUser)
        : IFacultyFacade
    {
        public async Task<bool> ExistsAsync(Guid facultyId, CancellationToken ct)
        {
            return await _db.faculties.AnyAsync(x => x.Id == facultyId);
        }

        public async Task<bool> IsAssetManagerOfFaculty(Guid userId, Guid facultyId)
        {
            if (isAdmin())
                return true;
            return await _db.asset_managers.AnyAsync(x => x.UserId == userId && x.FacultyId == facultyId);
        }

        public async Task<bool> IsBuildingLinkedAsync(Guid facultyId, Guid buildingId, CancellationToken ct)
        {
            return await _db.facultyBuildings
                .AnyAsync(x => x.FacultyId == facultyId && x.BuildingId == buildingId);
        }

        public async Task<bool> UserBelongsToFaculty(Guid userId, Guid facultyId, CancellationToken ct)
        {
            if(isAdmin())
            return true;

            return await 
                _db.asset_managers.AnyAsync(x => x.UserId == userId && x.FacultyId == facultyId)
                || await _db.students.AnyAsync(x => x.UserId == userId && x.FacultyId == facultyId)
                || await _db.teacherFaculties.AnyAsync(x => x.Teacher.UserId == userId && x.FacultyId == facultyId);
        }

        private bool isAdmin()
        {
            return _currentUser.Create().Role == Shared.Enums.Role.SuperAdmin;
        }
    }
}