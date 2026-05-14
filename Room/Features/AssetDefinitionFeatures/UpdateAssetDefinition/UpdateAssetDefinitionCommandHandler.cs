using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using Shared.Enums;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.AssetDefinitionFeatures.UpdateAssetDefinition
{
    public sealed record UpdateAssetDefinitionCommand(
        Guid Id,
        string Name,
        string SvgUrl,
        AssetCategory Category,
        List<PlacementLocation> Locations) : IRequest;

    internal sealed class UpdateAssetDefinitionCommandHandler(RoomDesignDbContext _db)
        : IRequestHandler<UpdateAssetDefinitionCommand>
    {
        public async Task Handle(UpdateAssetDefinitionCommand request, CancellationToken ct)
        {
            var definition = await _db.AssetDefinitions
                .FirstOrDefaultAsync(a => a.Id == request.Id, ct)
                ?? throw new DomainException("ASSET_DEF_NOT_FOUND", $"Asset definition {request.Id} not found.");

            definition.Update(request.Name, request.SvgUrl, request.Category, request.Locations);

            await _db.SaveChangesAsync(ct);
        }
    }
}
