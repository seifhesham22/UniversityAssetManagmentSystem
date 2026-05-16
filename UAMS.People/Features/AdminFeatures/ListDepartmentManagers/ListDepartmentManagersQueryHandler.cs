using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using UAMS.Campus.Presistence;
using UAMS.Identity.IdentityModels;

namespace UAMS.Campus.Features.AdminFeatures.ListDepartmentManagers
{
    public sealed record ListDepartmentManagersQuery(
        string? Search,
        Guid? DepartmentId,
        int Page = 1,
        int PageSize = 20
    ) : IRequest<PagedResult<DeptManagerAdminDto>>;

    public sealed record DeptManagerAdminDto(
        Guid Id,
        Guid UserId,
        string FullName,
        string Email,
        Guid DepartmentId,
        string DepartmentName,
        string Category
    );

    public sealed class ListDepartmentManagersQueryHandler(
        CampusDbContext _db,
        UserManager<User> _users)
        : IRequestHandler<ListDepartmentManagersQuery, PagedResult<DeptManagerAdminDto>>
    {
        public async Task<PagedResult<DeptManagerAdminDto>> Handle(
            ListDepartmentManagersQuery request,
            CancellationToken cancellationToken)
        {
            var q = _db.department_managers
                .AsNoTracking()
                .Include(dm => dm.Department)
                .AsQueryable();

            if (request.DepartmentId.HasValue)
                q = q.Where(dm => dm.DepartmentId == request.DepartmentId.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
                q = q.Where(dm => EF.Functions.ILike(dm.FullName, $"%{request.Search.Trim()}%"));

            var managers = await q.OrderBy(dm => dm.FullName).ToListAsync(cancellationToken);

            var userIds = managers.Select(dm => dm.UserId).Distinct().ToList();
            var emailMap = await _users.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email })
                .ToDictionaryAsync(u => u.Id, u => u.Email ?? "", cancellationToken);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim().ToLower();
                managers = managers.Where(dm =>
                    dm.FullName.Contains(request.Search.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    (emailMap.TryGetValue(dm.UserId, out var email) && email.Contains(term))
                ).ToList();
            }

            var total = managers.Count;
            var paged = managers
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(dm => new DeptManagerAdminDto(
                    dm.Id,
                    dm.UserId,
                    dm.FullName,
                    emailMap.GetValueOrDefault(dm.UserId, ""),
                    dm.DepartmentId,
                    dm.Department.Name,
                    dm.Department.Handles.ToString()
                ))
                .ToList();

            return new PagedResult<DeptManagerAdminDto>(paged, total, request.Page, request.PageSize);
        }
    }
}
