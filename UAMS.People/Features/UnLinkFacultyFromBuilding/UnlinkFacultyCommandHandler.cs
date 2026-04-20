using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.UnLinkFacultyFromBuilding
{
    public sealed record UnlinkFacultyCommand(Guid facultyId, Guid buildingId) : IRequest;
    public sealed class UnlinkFacultyCommandHandler(CampusDbContext _db)
        : IRequestHandler<UnlinkFacultyCommand>
    {
        public async Task Handle(UnlinkFacultyCommand request, CancellationToken cancellationToken)
        {
            var faculty = await _db
                .faculties
                .Include(x => x.BuildingLinks)
                .FirstOrDefaultAsync(x => x.Id == request.facultyId)
                ?? throw new InvalidOperationException("facultyNotFound");

            faculty.UnlinkBuilding(request.buildingId);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}