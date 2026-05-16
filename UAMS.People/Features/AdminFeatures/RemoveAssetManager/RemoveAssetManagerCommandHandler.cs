using MediatR;
using Microsoft.EntityFrameworkCore;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AdminFeatures.RemoveAssetManager
{
    public sealed record RemoveAssetManagerCommand(Guid AssetManagerId) : IRequest;

    public sealed class RemoveAssetManagerCommandHandler(CampusDbContext _db)
        : IRequestHandler<RemoveAssetManagerCommand>
    {
        public async Task Handle(RemoveAssetManagerCommand request, CancellationToken cancellationToken)
        {
            var manager = await _db.asset_managers
                .FirstOrDefaultAsync(am => am.Id == request.AssetManagerId, cancellationToken)
                ?? throw new InvalidOperationException("Asset manager not found.");

            _db.asset_managers.Remove(manager);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
