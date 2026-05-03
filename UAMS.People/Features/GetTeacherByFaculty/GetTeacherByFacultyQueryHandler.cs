using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.GetTeacherByFaculty
{
    public sealed record GetTeacherByFacultyQuery(Guid facultyId, int page = 1, int pageSize = 20)
        : IRequest<PagedResult<TeacherDto>>;

    public sealed record TeacherDto(Guid Id, Guid userId, string fullName, DateTime assignedAt);
    public sealed class GetTeacherByFacultyQueryHandler(CampusDbContext _db)
        : IRequestHandler<GetTeacherByFacultyQuery, PagedResult<TeacherDto>>
    {
        public async Task<PagedResult<TeacherDto>> Handle(GetTeacherByFacultyQuery request, CancellationToken cancellationToken)
        {
            if (await _db.faculties.AnyAsync(x => x.Id == request.facultyId))
                throw new InvalidOperationException("No faculty was found with this Id");

            var q = _db.teacherFaculties
                .AsNoTracking()
                .Where(x => x.FacultyId == request.facultyId)
                .Join(
                _db.teachers, tf => tf.TeacherId, t => t.Id,
                (tf,t) => new { t.Id, t.UserId, t.FullName, tf.AssignedAtUtc});

            var total = await q.CountAsync();
            var items = await q
                .Skip((1 - request.page) * request.pageSize)
                .Take(request.pageSize)
                .Select(x => new TeacherDto(x.Id, x.UserId, x.FullName, x.AssignedAtUtc))
                .ToListAsync();

            return new PagedResult<TeacherDto>(items, total, request.page, request.pageSize);
        }
    }
}