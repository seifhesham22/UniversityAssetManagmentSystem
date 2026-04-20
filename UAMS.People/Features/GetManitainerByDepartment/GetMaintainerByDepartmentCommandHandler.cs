using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.VisualBasic;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.GetManitainerByDepartment
{
    public sealed record GetMaintainerByDepartmentCommand(
        Guid departmentId, int page = 1, int pageSize = 20) : IRequest<PagedResult<MaintainerDto>>;

    public sealed record MaintainerDto(Guid id, string fullName,bool isActive ,Guid departmentId);
    public sealed class GetMaintainerByDepartmentCommandHandler(
        CampusDbContext _db) : IRequestHandler<GetMaintainerByDepartmentCommand, PagedResult<MaintainerDto>>
    {
        public async Task<PagedResult<MaintainerDto>> Handle(GetMaintainerByDepartmentCommand request, CancellationToken cancellationToken)
        {
            if (!await _db.departments.AnyAsync(x => x.Id == request.departmentId))
                throw new InvalidOperationException(
                    $"couldn't find a department with the Id {request.departmentId}"
                    );

            var query = _db.maintainers
                .AsNoTracking()
                .Where(x => x.DepartmentId == request.departmentId);
            
            var total = await query.CountAsync();
            var items = await query
                .Skip((request.page - 1) * request.pageSize)
                .Take(request.pageSize)
                .Select(x => new MaintainerDto(
                    id: x.Id,
                    fullName: x.FullName,
                    isActive: x.IsActive,
                    departmentId: request.departmentId))
                .ToListAsync(cancellationToken);

            return new PagedResult<MaintainerDto>(items, total, request.page, request.pageSize);
        }
    }
}