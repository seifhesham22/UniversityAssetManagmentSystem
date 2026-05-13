using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Models;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AdminFeatures.CreateDeapartment
{
    public sealed record CreateDepartmentCommand(
        string name,
        AssetCategory handles)
        : IRequest<Guid>;
    public sealed class CreateDepartmentCommandHandler(CampusDbContext _db)
        : IRequestHandler<CreateDepartmentCommand, Guid>
    {
        public async Task<Guid> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var exists = await _db.departments.AnyAsync(x => x.Name.Contains(request.name));
            if (exists)
                throw new InvalidOperationException("departmetn already exists");

            var department = new Department(
                name: request.name,
                handles: request.handles
                );

            await _db.AddAsync(department);
            await _db.SaveChangesAsync();

            return department.Id;
        }
    }
}