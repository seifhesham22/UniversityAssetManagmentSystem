using MediatR;
using Microsoft.EntityFrameworkCore;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AdminFeatures.RemoveDepartmentManager
{
    public sealed record RemoveDepartmentManagerCommand(Guid DepartmentManagerId) : IRequest;

    public sealed class RemoveDepartmentManagerCommandHandler(CampusDbContext _db)
        : IRequestHandler<RemoveDepartmentManagerCommand>
    {
        public async Task Handle(RemoveDepartmentManagerCommand request, CancellationToken ct)
        {
            var manager = await _db.department_managers
                .FirstOrDefaultAsync(dm => dm.Id == request.DepartmentManagerId, ct)
                ?? throw new InvalidOperationException("Department manager not found.");

            // Clear the department's manager reference so it is not left orphaned
            var dept = await _db.departments
                .FirstOrDefaultAsync(d => d.DepartmentManagerId == manager.Id, ct);
            if (dept is not null)
                dept.ClearManager();

            _db.department_managers.Remove(manager);
            await _db.SaveChangesAsync(ct);
        }
    }
}
