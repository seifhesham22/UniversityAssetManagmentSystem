using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.VisualBasic;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.DepartmentManagerFeatures.GetManitainerByDepartment
{
    public sealed record GetMaintainerByDepartmentCommand(
        Guid userId, int page = 1, int pageSize = 20) : IRequest<PagedResult<MaintainerDto>>;

    public sealed record MaintainerDto(Guid id, string fullName,bool isActive ,Guid departmentId);
    public sealed class GetMaintainerByDepartmentCommandHandler(
        CampusDbContext _db) : IRequestHandler<GetMaintainerByDepartmentCommand, PagedResult<MaintainerDto>>
    {
        public async Task<PagedResult<MaintainerDto>> Handle(GetMaintainerByDepartmentCommand request, CancellationToken cancellationToken)
        {
            var department = await _db.departments
                .FirstOrDefaultAsync(x => x.Manager.UserId == request.userId)
                 ?? throw new UnauthorizedAccessException(
                    $"Only department manager can do this"
                    );

            var query = _db.maintainers
                .AsNoTracking()
                .Where(x => x.DepartmentId == department.Id);
            
            var total = await query.CountAsync();
            var items = await query
                .Skip((request.page - 1) * request.pageSize)
                .Take(request.pageSize)
                .Select(x => new MaintainerDto(
                    id: x.Id,
                    fullName: x.FullName,
                    isActive: x.IsActive,
                    departmentId: department.Id))
                .ToListAsync(cancellationToken);

            return new PagedResult<MaintainerDto>(items, total, request.page, request.pageSize);
        }
    }
}