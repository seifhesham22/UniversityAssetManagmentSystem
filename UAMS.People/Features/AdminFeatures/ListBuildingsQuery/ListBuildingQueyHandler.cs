using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Models;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AdminFeatures.ListBuildingsQuery
{
    public sealed record ListBuildingQuery(string? search, int page = 1, int pageSize = 20)
        : IRequest<PagedResult<BuildingDto>>;

    public sealed record BuildingDto(Guid id, string name, string address);

    public sealed class ListBuildingQueyHandler(CampusDbContext _db)
        : IRequestHandler<ListBuildingQuery, PagedResult<BuildingDto>>
    {
        public async Task<PagedResult<BuildingDto>> Handle(ListBuildingQuery request, CancellationToken cancellationToken)
        {
            var q = _db.buildings.AsQueryable().AsNoTracking();

            if (!string.IsNullOrEmpty(request.search))
            {
                q = q.Where(x => x.Name.Contains(request.search.Trim()));
            }

            var total = await q.CountAsync(cancellationToken);
            var items = await q.OrderBy(x => x.Name)
                .Skip((request.page - 1) * request.pageSize).Take(request.pageSize)
                .Select(x => new BuildingDto(x.Id, x.Name, x.Address))
                .ToListAsync(cancellationToken);

            return new PagedResult<BuildingDto>(items, total, request.page, request.pageSize);
        }
    }
}