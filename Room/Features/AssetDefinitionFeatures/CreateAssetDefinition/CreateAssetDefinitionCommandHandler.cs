using MediatR;
using Microsoft.AspNetCore.Http.Features;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Models;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.AssetDefinitionFeatures.CreateAssetDefinition
{
    public sealed record CreateAssetDefinitionCommand(
        string name,
        string svgUrl,
        AssetCategory Category,
        List<PlacementLocation> Locations) : IRequest<Guid>;
    public sealed class CreateAssetDefinitionCommandHandler(RoomDesignDbContext _db)
        : IRequestHandler<CreateAssetDefinitionCommand, Guid>
    {
        public async Task<Guid> Handle(CreateAssetDefinitionCommand request, CancellationToken cancellationToken)
        {
            var newAssetDefinition = new AssetDefenetion(
                name: request.name,
                category: request.Category,
                placementLocations: request.Locations,
                svgUrl: request.svgUrl);

            await _db.AssetDefinitions.AddAsync(newAssetDefinition);
            await _db.SaveChangesAsync();

            return newAssetDefinition.Id;
        }
    }
}