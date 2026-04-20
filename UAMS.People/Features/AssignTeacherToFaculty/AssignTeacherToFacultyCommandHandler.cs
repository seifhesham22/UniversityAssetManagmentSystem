using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AssignTeacherToFaculty
{
    public sealed record AssignTeacherToFacultyCommand(Guid facultyId, Guid teacherId) : IRequest;
    public sealed class AssignTeacherToFacultyCommandHandler(CampusDbContext _db)
        : IRequestHandler<AssignTeacherToFacultyCommand>
    {
        public async Task Handle(AssignTeacherToFacultyCommand request, CancellationToken cancellationToken)
        {
            var teacher = await _db.teachers
                .Include(x => x.Faculties)
                .FirstOrDefaultAsync(x => x.Id == request.teacherId)
                ?? throw new InvalidOperationException(
                    $"couldn't find a teacher with the Id {request.teacherId}");

            if (await _db.faculties.AnyAsync(x => x.Id == request.facultyId))
                throw new InvalidOperationException(
                    $"Couldn't find a faculty with the Id {request.facultyId}");

            teacher.AssignToFaculty(request.facultyId);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}