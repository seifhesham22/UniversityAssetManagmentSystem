using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Models;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.CreateDeapartment
{
    public sealed record CreateDepartmentCommand(
        Guid departmentManagerId,
        string name,
        AssetCategory handles)
        : IRequest<Guid>;
    public sealed class CreateDepartmentCommandHandler(CampusDbContext _db)
        : IRequestHandler<CreateDepartmentCommand, Guid>
    {
        public async Task<Guid> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var manager = await _db
                .department_managers
                .FirstOrDefaultAsync(x => x.Id == request.departmentManagerId)
                ?? throw new InvalidOperationException(
                    $"Couldn't find departmentMnager with the Id {request.departmentManagerId}");

            var assigned = await _db
                .departments
                .AnyAsync(x => x.DepartmentManagerId == request.departmentManagerId);

            if(assigned)
                throw new InvalidOperationException(
                    "department manager already exists and assigned to another department");


            var department = new Department(
                name: request.name,
                handles: request.handles,
                departmentManagerId: request.departmentManagerId
                );

            await _db.AddAsync(department);
            manager.assignToDepartment(department.Id);
            await _db.SaveChangesAsync();

            return department.Id;
        }
    }
}