using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Features.AssetManagerFeatures.SearchTeachers;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AssetManagerFeatures.GetMyFacultyTeachers
{
    public sealed record TeacherDto(Guid Id, string name, string assignedAt);
    public sealed record GetMyFacultyTeachersQueryCommand(Guid userId) : IRequest<List<TeacherDto>>;
    public sealed class GetMyFacultyTeachersQueryHandler(CampusDbContext _db)
        : IRequestHandler<GetMyFacultyTeachersQueryCommand, List<TeacherDto>>
    {
        public async Task<List<TeacherDto>> Handle(GetMyFacultyTeachersQueryCommand request, CancellationToken cancellationToken)
        {
            var user = await _db
                .asset_managers
                .FirstOrDefaultAsync(x => x.UserId == request.userId)
                ?? throw new UnauthorizedAccessException("only asset manager can do this");

            return await _db
                .teacherFaculties
                .AsNoTracking()
                .Where(teacher => teacher.FacultyId == user.FacultyId)
                .OrderBy(tf => tf.Teacher.FullName)
                .Select(x => new TeacherDto(
                    x.TeacherId,
                    x.Teacher.FullName,
                    x.AssignedAtUtc.ToString()))
                .ToListAsync(cancellationToken);
        }
    }
}