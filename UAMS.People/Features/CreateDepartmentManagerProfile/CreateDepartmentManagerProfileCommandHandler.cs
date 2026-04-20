using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Models;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.CreateDepartmentManagerProfile
{
    public sealed record CreateDepartmentManagerCommand(
        Guid userId, Guid departmentId, string fullName) : IRequest<Guid>;
    public sealed class CreateDepartmentManagerProfileCommandHandler(
        CampusDbContext _db) : IRequestHandler<CreateDepartmentManagerCommand, Guid>
    {
        public async Task<Guid> Handle(CreateDepartmentManagerCommand request, CancellationToken cancellationToken)
        {
            if (await _db.department_managers.AnyAsync(x => x.UserId == request.userId))
                throw new InvalidOperationException("department manager profile already exists");

            if (!await _db.departments.AnyAsync(x => x.Id == request.departmentId))
                throw new InvalidOperationException(
                    $"couldn't find a department with the Id: {request.departmentId}"
                    );

            var newDepartmentManager = new DepartmentManager(
                userId: request.userId,
                departmentId: request.departmentId,
                fullName: request.fullName
                );

            await _db.AddAsync(newDepartmentManager);
            await _db.SaveChangesAsync(cancellationToken);

            return newDepartmentManager.Id;
        }
    }
}