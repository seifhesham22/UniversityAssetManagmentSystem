using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;
using UAMS.Room.Facades;

namespace UAMS.Campus.FacadeImplementations
{
    public sealed class FacultyFacade(CampusDbContext _db) : IFacultyFacade
    {
        public async Task<bool> ExistsAsync(Guid facultyId, CancellationToken ct)
        {
            return await _db.faculties.AnyAsync(x => x.Id == facultyId);
        }

        public async Task<bool> IsBuildingLinkedAsync(Guid facultyId, Guid buildingId, CancellationToken ct)
        {
            return await _db.facultyBuildings
                .AnyAsync(x => x.FacultyId == facultyId && x.BuildingId == buildingId);
        }
    }
}