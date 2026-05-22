using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.DepartmentManagerFeatures.RemoveMaintainer
{
    public sealed record RemoveMaintainerCommand(Guid DeptManagerUserId, Guid MaintainerId) : IRequest<Guid>;

    public sealed class RemoveMaintainerCommandHandler(CampusDbContext _db)
        : IRequestHandler<RemoveMaintainerCommand, Guid>
    {
        public async Task<Guid> Handle(RemoveMaintainerCommand request, CancellationToken ct)
        {
            var manager = await _db.department_managers
                .FirstOrDefaultAsync(dm => dm.UserId == request.DeptManagerUserId, ct)
                ?? throw new DomainException("DEPT_MANAGER_NOT_FOUND", "Department manager not found.");

            var maintainer = await _db.maintainers
                .FirstOrDefaultAsync(m => m.Id == request.MaintainerId && m.DepartmentId == manager.DepartmentId, ct)
                ?? throw new DomainException("MAINTAINER_NOT_FOUND", "Maintainer not found in your department.");

            var userId = maintainer.UserId;
            _db.maintainers.Remove(maintainer);
            await _db.SaveChangesAsync(ct);
            return userId;
        }
    }
}
