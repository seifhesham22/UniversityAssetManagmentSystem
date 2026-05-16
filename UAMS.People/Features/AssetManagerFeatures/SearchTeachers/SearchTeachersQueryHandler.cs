using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AssetManagerFeatures.SearchTeachers
{
    public sealed record SearchTeachersFacultyQuery(
        string search,
        bool unassigned,
        Guid? ExcludeFacultyId = null,
        int page = 1,
        int pageSize = 20)
        : IRequest<PagedResult<TeacherSearchDto>>;

    public sealed record TeacherSearchDto(Guid Id, string fullName);
    public sealed class SearchTeachersQueryHandler(CampusDbContext _db)
        : IRequestHandler<SearchTeachersFacultyQuery, PagedResult<TeacherSearchDto>>
    {
        public async Task<PagedResult<TeacherSearchDto>> Handle(SearchTeachersFacultyQuery request, CancellationToken cancellationToken)
        {
            var q = _db.teachers
                .AsNoTracking()
                .Where(t => EF.Functions.ILike(t.FullName, $"%{request.search}%"));

            if (request.unassigned)
                q = q.Where(t => !t.Faculties.Any());

            // Always exclude teachers already in the caller's faculty
            if (request.ExcludeFacultyId.HasValue)
                q = q.Where(t => !t.Faculties.Any(tf => tf.FacultyId == request.ExcludeFacultyId.Value));

            var total = await q.CountAsync(cancellationToken);
            var items = await q
                .OrderBy(t => t.FullName)
                .Skip((request.page - 1) * request.pageSize)
                .Take(request.pageSize)
                .Select(x => new TeacherSearchDto(x.Id, x.FullName))
                .ToListAsync(cancellationToken);

            return new PagedResult<TeacherSearchDto>(items, total, request.page, request.pageSize);
        }
    }
}
