using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.ListFacultiesQuery
{
    public sealed record ListFacultiesQuery(string? search, int page = 1, int pageSize = 20)
        : IRequest<PagedResult<FacultyDto>>;

    public sealed record FacultyDto(Guid id, string name);
    public sealed class ListFacultiesQueryHandler(CampusDbContext _db)
        : IRequestHandler<ListFacultiesQuery, PagedResult<FacultyDto>>
    {
        public async Task<PagedResult<FacultyDto>> Handle(ListFacultiesQuery request, CancellationToken cancellationToken)
        {
            var q = _db.faculties.AsQueryable().AsNoTracking();
            if (!string.IsNullOrEmpty(request.search))
            {
                q = q.Where(x => x.Name.Contains(request.search.Trim()));
            }

            var total = await q.CountAsync();
            var items = await q.OrderBy(x => x.Name)
                .Skip((1 - request.page) * request.pageSize)
                .Take(request.pageSize)
                .Select(x => new FacultyDto(x.Id, x.Name))
                .ToListAsync(cancellationToken);

            return new PagedResult<FacultyDto>(items, total, request.page, request.pageSize);
        }
    }
}