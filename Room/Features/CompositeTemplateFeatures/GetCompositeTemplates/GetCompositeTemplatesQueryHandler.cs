using MediatR;
using Microsoft.EntityFrameworkCore;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.CompositeTemplateFeatures.GetCompositeTemplates
{
    public sealed record GetCompositeTemplatesQuery : IRequest<List<CompositeTemplateView>>;

    public sealed record CompositeTemplateView(
        Guid Id,
        string Name,
        List<CompositeItemView> Items);

    public sealed record CompositeItemView(
        Guid AssetDefinitionId,
        string AssetName,
        float RelX, float RelY,
        float Width, float Height,
        float Rotation);

    internal sealed class GetCompositeTemplatesQueryHandler(RoomDesignDbContext _db)
        : IRequestHandler<GetCompositeTemplatesQuery, List<CompositeTemplateView>>
    {
        public async Task<List<CompositeTemplateView>> Handle(
            GetCompositeTemplatesQuery request, CancellationToken ct)
        {
            var templates = await _db.CompositeTemplates
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync(ct);

            var defIds = templates
                .SelectMany(t => t.Items.Select(i => i.AssetDefinitionId))
                .Distinct()
                .ToList();

            var defNames = await _db.AssetDefinitions
                .Where(d => defIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.Name, ct);

            return templates.Select(t => new CompositeTemplateView(
                t.Id, t.Name,
                t.Items.Select(i => new CompositeItemView(
                    i.AssetDefinitionId,
                    defNames.GetValueOrDefault(i.AssetDefinitionId, "Unknown"),
                    i.RelX, i.RelY,
                    i.Width, i.Height,
                    i.Rotation)).ToList()
            )).ToList();
        }
    }
}
