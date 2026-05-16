using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.AssetDefinitionFeatures.AddCheckListItemToAsset
{
    public sealed record AddCheckListItemToAssetCommand(Guid AssetDefinitionId, string Description) : IRequest<Guid>;
    public sealed class AddCheckListItemToAssetCommandHandler(RoomDesignDbContext _db)
        : IRequestHandler<AddCheckListItemToAssetCommand, Guid>
    {
        public async Task<Guid> Handle(AddCheckListItemToAssetCommand request, CancellationToken cancellationToken)
        {
            var assetDefinition = await _db.AssetDefinitions
                .FirstOrDefaultAsync(x => x.Id == request.AssetDefinitionId)
                ?? throw new InvalidOperationException(
                    $"couldn't find asset definition with the Id {request.AssetDefinitionId}");

            var checklistItemTemplate = assetDefinition.AddChecklistItem(request.Description);
            await _db.CheckListItemTemplates.AddAsync(checklistItemTemplate, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return checklistItemTemplate.Id;
        }
    }
}