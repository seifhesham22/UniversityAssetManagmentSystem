using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Models;
using UAMS.Campus.Presistence;
using UAMS.Identity.IdentityModels;

namespace UAMS.Campus.Features.DepartmentManagerFeatures.CreateManitainerProfile
{
    public sealed record CreateMaintainerProfileCommand(
        Guid userId,
        string fullName) : IRequest<Guid>;
    public sealed class CreateMaintainerProfileCommandHandler(CampusDbContext _db)
        : IRequestHandler<CreateMaintainerProfileCommand, Guid>
    {
        public async Task<Guid> Handle(
            CreateMaintainerProfileCommand request,
            CancellationToken cancellationToken)
        {
            var departmentManager = await _db.department_managers
                .FirstOrDefaultAsync(x => x.UserId == request.userId);

            if (departmentManager is null)
                throw new UnauthorizedAccessException("only dep manager can do this");

            var maintainerProfile = new Maintainer(
                userId: request.userId,
                fullName: request.fullName,
                departmentId: departmentManager.DepartmentId);

            await _db.maintainers.AddAsync(maintainerProfile);
            await _db.SaveChangesAsync();

            return maintainerProfile.Id;
        }
    }
}