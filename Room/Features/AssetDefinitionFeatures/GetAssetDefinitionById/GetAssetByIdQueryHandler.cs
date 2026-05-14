using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Presistence;
using UAMS.Room.ViewDtos;

namespace UAMS.Room.Features.AssetDefinitionFeatures.GetAssetDefinitionById
{
    public sealed record GetAssetDefinitionByIdCommand(Guid Id) : IRequest<AssetDefinitionDetailView>;
    public sealed class GetAssetByIdQueryHandler(RoomDesignDbContext _db)
        : IRequestHandler<GetAssetDefinitionByIdCommand, AssetDefinitionDetailView>
    {
        public async Task<AssetDefinitionDetailView> Handle(GetAssetDefinitionByIdCommand request, CancellationToken cancellationToken)
        {
            var assetDefenition = await _db.AssetDefinitions
                .Include(x => x.AllowedLocations)
                .Include(x => x.ChecklistTemplate)
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                ?? throw new InvalidOperationException($"couldn't find an asset definition with the Id {request.Id}");

            return new AssetDefinitionDetailView(
                assetDefenition.Id,
                assetDefenition.Name,
                assetDefenition.Category.ToString(),
                assetDefenition.SvgUrl,
                assetDefenition.AllowedLocations.Select(location => location.ToString()).ToList(),
                assetDefenition.ChecklistTemplate
                .Select(checklistItem =>
                new ChecklistItemView(checklistItem.Id, checklistItem.Description))
                .ToList());
        }
    }
}