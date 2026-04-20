using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.RemoveTeacherFromFaculty
{
    public sealed record RemoveTeacherFromFacultyCommand(Guid facultyId, Guid teacherId) : IRequest;
    public sealed class RemoveTeacherFromFacultyCommandHandler(CampusDbContext _db)
        : IRequestHandler<RemoveTeacherFromFacultyCommand>
    {
        public async Task Handle(RemoveTeacherFromFacultyCommand request, CancellationToken cancellationToken)
        {
            if (await _db.faculties.AnyAsync(x => x.Id == request.facultyId))
                throw new InvalidOperationException("couldnt find a faculty with the provided Id");

            var teacher = await _db.teachers
                .Include(x => x.Faculties)
                .FirstOrDefaultAsync(x => x.Id == request.teacherId)
                ?? throw new InvalidOperationException(
                    $"couldn't finad a teacher with this id {request.teacherId}");

            teacher.RemoveFromFaculty(request.facultyId);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}