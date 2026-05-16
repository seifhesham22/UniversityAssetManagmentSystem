using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AdminFeatures.ListAdminFaculties
{
    public sealed record ListAdminFacultiesQuery(string? Search, int Page = 1, int PageSize = 20)
        : IRequest<PagedResult<AdminFacultyDto>>;

    public sealed record AdminBuildingDto(Guid Id, string Name, string Address);

    public sealed record AdminFacultyDto(
        Guid Id,
        string Name,
        bool IsActive,
        List<AdminBuildingDto> Buildings,
        string? AssetManagerName);

    public sealed class ListAdminFacultiesQueryHandler(CampusDbContext _db)
        : IRequestHandler<ListAdminFacultiesQuery, PagedResult<AdminFacultyDto>>
    {
        public async Task<PagedResult<AdminFacultyDto>> Handle(
            ListAdminFacultiesQuery request,
            CancellationToken cancellationToken)
        {
            var q = _db.faculties.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
                q = q.Where(f => f.Name.Contains(request.Search.Trim()));

            var total = await q.CountAsync(cancellationToken);

            var items = await q
                .OrderBy(f => f.Name)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(f => new AdminFacultyDto(
                    f.Id,
                    f.Name,
                    f.IsActive,
                    f.BuildingLinks
                        .Select(bl => new AdminBuildingDto(bl.BuildingId, bl.Building.Name, bl.Building.Address))
                        .ToList(),
                    f.AssetManagers.Select(am => am.FullName).FirstOrDefault()
                ))
                .ToListAsync(cancellationToken);

            return new PagedResult<AdminFacultyDto>(items, total, request.Page, request.PageSize);
        }
    }
}
