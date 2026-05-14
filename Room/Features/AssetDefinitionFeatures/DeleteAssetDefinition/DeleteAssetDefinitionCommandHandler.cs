using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.AssetDefinitionFeatures.DeleteAssetDefinition
{
    public sealed record DeleteAssetDefinitionCommand(Guid Id) : IRequest;

    internal sealed class DeleteAssetDefinitionCommandHandler(RoomDesignDbContext _db)
        : IRequestHandler<DeleteAssetDefinitionCommand>
    {
        public async Task Handle(DeleteAssetDefinitionCommand request, CancellationToken ct)
        {
            var definition = await _db.AssetDefinitions
                .Include(a => a.ChecklistTemplate)
                .FirstOrDefaultAsync(a => a.Id == request.Id, ct)
                ?? throw new DomainException("ASSET_DEF_NOT_FOUND", $"Asset definition {request.Id} not found.");

            var inUse = await _db.Rooms
                .AnyAsync(r => r.Layout.PlacedAssets.Any(pa => pa.AssetDefinitionId == request.Id), ct);

            if (inUse)
                throw new DomainException("ASSET_DEF_IN_USE",
                    "This asset definition is in use by one or more room layouts and cannot be deleted.");

            _db.AssetDefinitions.Remove(definition);
            await _db.SaveChangesAsync(ct);
        }
    }
}
