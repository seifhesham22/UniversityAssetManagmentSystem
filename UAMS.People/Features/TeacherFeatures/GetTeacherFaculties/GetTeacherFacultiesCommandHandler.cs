using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Shared.Authorization;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.TeacherFeatures.GetTeacherFaculties
{
   public sealed record GetTeacherFacultiesCommand(Guid userId) : IRequest<List<TeacherFacultyDto>>;

   public sealed record TeacherFacultyDto(
       Guid FacultyId,
       string FacultyName,
       DateTime AssignedAtUtc);

    public sealed class GetTeacherFacultiesCommandHandler(CampusDbContext _db)
        : IRequestHandler<GetTeacherFacultiesCommand, List<TeacherFacultyDto>>
    {
        public async Task<List<TeacherFacultyDto>> Handle(GetTeacherFacultiesCommand request, CancellationToken cancellationToken)
        {
            var teacherId = await _db.teachers
                .AsNoTracking()
                .Where(x => x.UserId == request.userId)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (teacherId == Guid.Empty)
                return new List<TeacherFacultyDto>();

            return await _db.teacherFaculties
                .AsNoTracking()
                .Where(tf => tf.TeacherId == teacherId)
                .OrderBy(tf => tf.Faculty.Name)
                .Select(tf => new TeacherFacultyDto(
                    tf.Faculty.Id,
                    tf.Faculty.Name,
                    tf.AssignedAtUtc
                ))
                .ToListAsync(cancellationToken);
        }
    }
}