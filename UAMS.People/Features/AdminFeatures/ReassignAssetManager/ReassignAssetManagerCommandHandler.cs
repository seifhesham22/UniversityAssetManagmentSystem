using MediatR;
using Microsoft.EntityFrameworkCore;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AdminFeatures.ReassignAssetManager
{
    public sealed record ReassignAssetManagerCommand(Guid AssetManagerId, Guid NewFacultyId) : IRequest;

    public sealed class ReassignAssetManagerCommandHandler(CampusDbContext _db)
        : IRequestHandler<ReassignAssetManagerCommand>
    {
        public async Task Handle(ReassignAssetManagerCommand request, CancellationToken cancellationToken)
        {
            var manager = await _db.asset_managers
                .FirstOrDefaultAsync(am => am.Id == request.AssetManagerId, cancellationToken)
                ?? throw new InvalidOperationException("Asset manager not found.");

            if (!await _db.faculties.AnyAsync(f => f.Id == request.NewFacultyId, cancellationToken))
                throw new InvalidOperationException("Faculty not found.");

            manager.ReassignToFaculty(request.NewFacultyId);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
