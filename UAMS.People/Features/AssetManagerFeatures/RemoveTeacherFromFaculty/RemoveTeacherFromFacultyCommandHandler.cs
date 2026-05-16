using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AssetManagerFeatures.RemoveTeacherFromFaculty
{
    public sealed record RemoveTeacherFromFacultyCommand(Guid teacherId, Guid userId) : IRequest;
    public sealed class RemoveTeacherFromFacultyCommandHandler(CampusDbContext _db)
        : IRequestHandler<RemoveTeacherFromFacultyCommand>
    {
        public async Task Handle(RemoveTeacherFromFacultyCommand request, CancellationToken cancellationToken)
        {
            var assetManager = await _db
                .asset_managers
                .FirstOrDefaultAsync(x => x.UserId == request.userId)
                ?? throw new UnauthorizedAccessException("Only assetManager Can Do This");

            var teacher = await _db.teachers
                .Include(t => t.Faculties)
                .FirstOrDefaultAsync(x => x.Id == request.teacherId)
                ?? throw new InvalidOperationException($"couldn't find a teacher with the Id {request.teacherId}");

            teacher.RemoveFromFaculty(assetManager.FacultyId);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}