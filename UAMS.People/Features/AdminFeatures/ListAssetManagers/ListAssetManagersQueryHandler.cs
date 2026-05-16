using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using UAMS.Campus.Presistence;
using UAMS.Identity.IdentityModels;

namespace UAMS.Campus.Features.AdminFeatures.ListAssetManagers
{
    public sealed record ListAssetManagersQuery(
        string? Search,
        Guid? FacultyId,
        int Page = 1,
        int PageSize = 20
    ) : IRequest<PagedResult<AssetManagerAdminDto>>;

    public sealed record AssetManagerAdminDto(
        Guid Id,
        Guid UserId,
        string FullName,
        string Email,
        Guid FacultyId,
        string FacultyName
    );

    public sealed class ListAssetManagersQueryHandler(
        CampusDbContext _db,
        UserManager<User> _users)
        : IRequestHandler<ListAssetManagersQuery, PagedResult<AssetManagerAdminDto>>
    {
        public async Task<PagedResult<AssetManagerAdminDto>> Handle(
            ListAssetManagersQuery request,
            CancellationToken cancellationToken)
        {
            // 1. Query campus managers (filter by faculty and name if provided)
            var q = _db.asset_managers
                .AsNoTracking()
                .Include(am => am.Faculty)
                .AsQueryable();

            if (request.FacultyId.HasValue)
                q = q.Where(am => am.FacultyId == request.FacultyId.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
                q = q.Where(am => EF.Functions.ILike(am.FullName, $"%{request.Search.Trim()}%"));

            var managers = await q.OrderBy(am => am.FullName).ToListAsync(cancellationToken);

            // 2. Look up emails from identity DB for all relevant UserIds
            var userIds = managers.Select(am => am.UserId).Distinct().ToList();
            var emailMap = await _users.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email })
                .ToDictionaryAsync(u => u.Id, u => u.Email ?? "", cancellationToken);

            // 3. If searching by email, filter further in memory
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim().ToLower();
                managers = managers.Where(am =>
                    am.FullName.Contains(request.Search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    (emailMap.TryGetValue(am.UserId, out var email) && email.Contains(term))
                ).ToList();
            }

            var total = managers.Count;
            var paged = managers
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(am => new AssetManagerAdminDto(
                    am.Id,
                    am.UserId,
                    am.FullName,
                    emailMap.GetValueOrDefault(am.UserId, ""),
                    am.FacultyId,
                    am.Faculty.Name
                ))
                .ToList();

            return new PagedResult<AssetManagerAdminDto>(paged, total, request.Page, request.PageSize);
        }
    }
}
