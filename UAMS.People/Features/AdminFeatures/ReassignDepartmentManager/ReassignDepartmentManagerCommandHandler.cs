using MediatR;
using Microsoft.EntityFrameworkCore;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AdminFeatures.ReassignDepartmentManager
{
    public sealed record ReassignDepartmentManagerCommand(
        Guid DepartmentManagerId,
        Guid NewDepartmentId) : IRequest;

    public sealed class ReassignDepartmentManagerCommandHandler(CampusDbContext _db)
        : IRequestHandler<ReassignDepartmentManagerCommand>
    {
        public async Task Handle(ReassignDepartmentManagerCommand request, CancellationToken ct)
        {
            var manager = await _db.department_managers
                .FirstOrDefaultAsync(dm => dm.Id == request.DepartmentManagerId, ct)
                ?? throw new InvalidOperationException("Department manager not found.");

            if (!await _db.departments.AnyAsync(d => d.Id == request.NewDepartmentId, ct))
                throw new InvalidOperationException("Department not found.");

            // Unlink from old department
            var oldDept = await _db.departments
                .FirstOrDefaultAsync(d => d.DepartmentManagerId == manager.Id, ct);
            if (oldDept is not null)
                oldDept.ClearManager();

            // Assign to new department
            var newDept = await _db.departments
                .FirstOrDefaultAsync(d => d.Id == request.NewDepartmentId, ct);
            newDept!.ChangeManager(manager.Id);

            manager.ReassignToDepartment(request.NewDepartmentId);

            await _db.SaveChangesAsync(ct);
        }
    }
}
