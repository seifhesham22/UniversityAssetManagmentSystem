using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace UAMS.Campus.Features.All.ListDepartments
{
    public sealed record ListDepartmentCommand(
        string? search,
        AssetCategory? categoryFilter,
        int page = 1,
        int pageSize = 20)
        : IRequest<PagedResult<DepartmentDto>>;

    public sealed record DepartmentDto(Guid Id ,string name, AssetCategory handles);

    public sealed class ListDepartmentsQueryHandler(CampusDbContext _db)
        : IRequestHandler<ListDepartmentCommand, PagedResult<DepartmentDto>>
    {
        public async Task<PagedResult<DepartmentDto>> Handle(ListDepartmentCommand request, CancellationToken cancellationToken)
        {
            var q = _db.departments.AsQueryable().AsNoTracking();

            if (!string.IsNullOrEmpty(request.search))
            {
                q = q.Where(x => x.Name.Contains(request.search.Trim()));
            }

            if (request.categoryFilter.HasValue)
            {
                q = q.Where(x => x.Handles == request.categoryFilter.Value);
            }

            var total = await q.CountAsync(cancellationToken);
            var items = await q.Skip((1 - request.page) * request.pageSize)
                .Take(request.pageSize)
                .Select(x => new DepartmentDto(x.Id, x.Name, x.Handles))
                .ToListAsync();

            return new PagedResult<DepartmentDto>(items, total, request.page, request.pageSize);
        }
    }
}