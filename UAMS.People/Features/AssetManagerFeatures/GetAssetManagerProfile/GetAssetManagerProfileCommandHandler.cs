using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AssetManagerFeatures.GetAssetManagerProfile
{
    public sealed record AssetManagerDto(Guid Id, Guid userId, Guid facultyId, string name);
    public sealed record GetAssetManagerProfileCommand(Guid userId) : IRequest<AssetManagerDto>;
    public sealed class GetAssetManagerProfileCommandHandler(CampusDbContext _db)
        : IRequestHandler<GetAssetManagerProfileCommand, AssetManagerDto>
    {
        public async Task<AssetManagerDto> Handle(GetAssetManagerProfileCommand request, CancellationToken cancellationToken)
        {
            var assetManager = await _db.asset_managers
                .FirstOrDefaultAsync(x => x.UserId == request.userId)
                ?? throw new UnauthorizedAccessException("you are not asset manager");

            return new AssetManagerDto(assetManager.Id, assetManager.UserId, assetManager.FacultyId, assetManager.FullName);
        }
    }
}
