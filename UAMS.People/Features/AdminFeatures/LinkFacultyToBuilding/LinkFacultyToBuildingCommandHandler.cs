using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AdminFeatures.LinkFacultyToBuilding
{
    public sealed record LinkFacultyToBuildingCommand(Guid buildingId, Guid facultyId) : IRequest;
    public sealed class LinkFacultyToBuildingCommandHandler(CampusDbContext _db)
        : IRequestHandler<LinkFacultyToBuildingCommand>
    {
        public async Task Handle(LinkFacultyToBuildingCommand request, CancellationToken cancellationToken)
        {
            var faculty = await _db.faculties
                .Include(x => x.BuildingLinks)
                .FirstOrDefaultAsync(x => x.Id == request.facultyId)
                ?? throw new InvalidOperationException("faculty not found");

            if (!await _db.buildings.AnyAsync(x => x.Id == request.buildingId))
                throw new InvalidOperationException("building not found");

            faculty.LinkToBuilding(request.buildingId);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}