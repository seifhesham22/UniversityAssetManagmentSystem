using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Features.AdminFeatures.ListBuildingsQuery;
using UAMS.Campus.Features.TeacherFeatures.GetTeacherFaculties;
using UAMS.Campus.Models;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.All.GetBuildingsAssociatedToFaculty
{
    public sealed record GetFacultyBuildingQueryCommand(Guid facultyId) : IRequest<List<BuildingDto>>;
    public sealed class GetFacultyBuildingQueryHandler(CampusDbContext _db)
        : IRequestHandler<GetFacultyBuildingQueryCommand, List<BuildingDto>>
    {
        public async Task<List<BuildingDto>> Handle(GetFacultyBuildingQueryCommand request, CancellationToken cancellationToken)
        {
            var Facultybuildings = await _db
                .facultyBuildings
                .Include(x => x.Building)
                .Where(x => x.FacultyId == request.facultyId)
                .ToListAsync();

            return Facultybuildings
                .Select(x => new BuildingDto(
                    x.BuildingId,
                    x.Building.Name,
                    x.Building.Address))
                .ToList();
        }
    }
}