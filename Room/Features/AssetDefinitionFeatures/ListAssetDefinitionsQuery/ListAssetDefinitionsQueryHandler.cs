using MediatR;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Presistence;
using UAMS.Room.ViewDtos;

namespace UAMS.Room.Features.AssetDefinitionFeatures.ListAssetDefinitionsQuery
{
    public sealed record ListAssetDefinitionsQueryCommand(
        string? search,
        AssetCategory? assetCategory,
        int page = 1,
        int totalSize = 20)
        : IRequest<PagedResult<AssetDefinitionListItem>>;
    public sealed class ListAssetDefinitionsQueryHandler(RoomDesignDbContext _db)
        : IRequestHandler<ListAssetDefinitionsQueryCommand, PagedResult<AssetDefinitionListItem>>
    {
        public async Task<PagedResult<AssetDefinitionListItem>> Handle(
            ListAssetDefinitionsQueryCommand request,
            CancellationToken cancellationToken)
        {
            var assets = _db.AssetDefinitions
                .AsQueryable()
                .AsNoTracking();

            if (!string.IsNullOrEmpty(request.search))
            {
                assets = assets.Where(x => EF.Functions.ILike(x.Name, $"%{request.search}%"));
            }

            if (request.assetCategory.HasValue)
            {
                assets = assets
                    .Where(x => x.Category == request.assetCategory.Value);
            }

            var total = await assets.CountAsync(cancellationToken);

            var entities = await assets
                .OrderBy(x => x.Name)
                .Skip((request.page - 1) * request.totalSize)
                .Take(request.totalSize)
                .ToListAsync(cancellationToken);

            var items = entities.Select(x => new AssetDefinitionListItem(
                x.Id,
                x.Name,
                x.Category.ToString(),
                x.SvgUrl,
                x.AllowedLocations.Select(l => l.ToString()).ToList()
            )).ToList();

            return new PagedResult<AssetDefinitionListItem>(items, total, request.page, request.totalSize);
        }
    }
}