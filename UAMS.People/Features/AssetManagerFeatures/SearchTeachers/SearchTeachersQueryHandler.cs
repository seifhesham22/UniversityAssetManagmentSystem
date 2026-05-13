using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AssetManagerFeatures.SearchTeachers
{
    public sealed record SearchTeachersFacultyQuery(string search, bool unassigned, int page = 1, int pageSize = 20)
        : IRequest<PagedResult<TeacherSearchDto>>;

    public sealed record TeacherSearchDto(Guid Id, string fullName);
    public sealed class SearchTeachersQueryHandler(CampusDbContext _db)
        : IRequestHandler<SearchTeachersFacultyQuery, PagedResult<TeacherSearchDto>>
    {
        public async Task<PagedResult<TeacherSearchDto>> Handle(SearchTeachersFacultyQuery request, CancellationToken cancellationToken)
        {
            var q = _db.teachers
            .AsNoTracking()
            .Where(teacher => EF.Functions.ILike(teacher.FullName, $"%{request.search}%"));

            if (request.unassigned)
            {
                q = q.Where(t => !t.Faculties.Any());
            }

            var total = await q.CountAsync();
            var items = await q
                .Skip((request.page - 1) * request.pageSize)
                .Take(request.pageSize)
                .Select(x => new TeacherSearchDto(x.Id, x.FullName))
                .ToListAsync();

            return new PagedResult<TeacherSearchDto>(items, total, request.page, request.pageSize);
        }
    }
}