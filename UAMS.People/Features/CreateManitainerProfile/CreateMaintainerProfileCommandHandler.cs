using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Models;
using UAMS.Campus.Presistence;
using UAMS.Identity.IdentityModels;

namespace UAMS.Campus.Features.CreateManitainerProfile
{
    public sealed record CreateMaintainerProfileCommand(
        Guid userId,
        Guid departmentId,
        string fullName) : IRequest<Guid>;
    public sealed class CreateMaintainerProfileCommandHandler(CampusDbContext _db)
        : IRequestHandler<CreateMaintainerProfileCommand, Guid>
    {
        public async Task<Guid> Handle(CreateMaintainerProfileCommand request, CancellationToken cancellationToken)
        {
            var maintainer = await _db.maintainers
                .FirstOrDefaultAsync(x => x.UserId == request.userId);

            if (maintainer is not null)
                throw new InvalidOperationException("maintainer already exists");

            var res = await _db.departments.AnyAsync(x => x.Id == request.departmentId);
            if (!res)
                throw new InvalidOperationException(
                    $"couldn't find department with id {request.departmentId}");

            var maintainerProfile = new Maintainer(
                userId: request.userId,
                fullName: request.fullName,
                departmentId: request.departmentId);

            await _db.maintainers.AddAsync(maintainerProfile);
            await _db.SaveChangesAsync();

            return maintainerProfile.Id;
        }
    }
}