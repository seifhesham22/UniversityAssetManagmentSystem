using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.AssetDefinitionFeatures.RemoveCheckListItemFromAsset
{
    public sealed record RemoveAssetCheckListItemFromAssetDefinitionCommand(Guid AssetDefinitionId, Guid CheckListItemId) : IRequest;
    public sealed class RemoveCheckListItemFromAssetDefinitionCommandHandler(RoomDesignDbContext _db)
        : IRequestHandler<RemoveAssetCheckListItemFromAssetDefinitionCommand>
    {
        public async Task Handle(
            RemoveAssetCheckListItemFromAssetDefinitionCommand request,
            CancellationToken cancellationToken)
        {
            var assetDefenetion = await _db.AssetDefinitions
                .Include(x => x.ChecklistTemplate)
                .FirstOrDefaultAsync(x => x.Id == request.AssetDefinitionId, cancellationToken)
                ?? throw new InvalidOperationException($"couldn't find an asset definition with the Id {request.AssetDefinitionId}");

            assetDefenetion.RemoveChecklistItem(request.CheckListItemId);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}