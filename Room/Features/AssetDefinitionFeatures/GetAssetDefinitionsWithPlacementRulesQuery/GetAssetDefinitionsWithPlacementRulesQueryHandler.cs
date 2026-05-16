using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Presistence;
using UAMS.Room.ViewDtos;

namespace UAMS.Room.Features.AssetDefinitionFeatures.GetAssetDefinitionsWithPlacementRulesQuery
{
    public sealed record GetAssetDefinitionsWithPlacementRulesCommand : IRequest<PlacementRulesView>;
    public sealed class GetAssetDefinitionsWithPlacementRulesQueryHandler(RoomDesignDbContext _db)
        : IRequestHandler<GetAssetDefinitionsWithPlacementRulesCommand, PlacementRulesView>
    {
        public async Task<PlacementRulesView> Handle(
            GetAssetDefinitionsWithPlacementRulesCommand request,
            CancellationToken cancellationToken)
        {
            var assets = await _db.AssetDefinitions
                .AsNoTracking()
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var groups = assets
                .GroupBy(x => x.Category)
                .Select(x => new CategoryAssetsDto(
                    Category: x.Key.ToString(),
                    AssetDefinitions: x.Select(x => new AssetDto(
                        x.Id,
                        x.Name,
                        x.SvgUrl,
                        x.AllowedLocations.Select(location => location.ToString()).ToList()
                        )).ToList()
                    ))
                .OrderBy(g => g.Category)
                .ToList();

            return new PlacementRulesView(groups);
        }
    }
}