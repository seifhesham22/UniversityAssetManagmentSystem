using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AssetManagerFeatures.AssignTeacherToFaculty
{
    public sealed record AssignTeacherToFacultyCommand(Guid teacherId, Guid userId) : IRequest;
    public sealed class AssignTeacherToFacultyCommandHandler(CampusDbContext _db)
        : IRequestHandler<AssignTeacherToFacultyCommand>
    {
        public async Task Handle(AssignTeacherToFacultyCommand request, CancellationToken cancellationToken)
        {
            var assetManager = await _db.asset_managers
                .FirstOrDefaultAsync(x => x.UserId == request.userId)
                ?? throw new UnauthorizedAccessException("only the asset manager can do this");

            var teacher = await _db.teachers
                .FirstOrDefaultAsync(x => x.Id == request.teacherId)
                ?? throw new InvalidOperationException(
                    $"couldn't find a teacher with the Id {request.teacherId}");

            teacher.AssignToFaculty(assetManager.FacultyId);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}