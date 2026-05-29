using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Models;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.CompositeTemplateFeatures.CreateCompositeTemplate
{
    public sealed record CreateCompositeTemplateItemRequest(
        Guid AssetDefinitionId,
        float RelX, float RelY,
        float Width, float Height,
        float Rotation);

    public sealed record CreateCompositeTemplateCommand(
        string Name,
        List<CreateCompositeTemplateItemRequest> Items) : IRequest<Guid>;

    internal sealed class CreateCompositeTemplateCommandHandler(RoomDesignDbContext _db)
        : IRequestHandler<CreateCompositeTemplateCommand, Guid>
    {
        public async Task<Guid> Handle(CreateCompositeTemplateCommand request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new DomainException("NAME_REQUIRED", "Template name is required.");

            if (request.Items == null || request.Items.Count == 0)
                throw new DomainException("ITEMS_REQUIRED", "At least one item is required.");

            var defIds = request.Items.Select(i => i.AssetDefinitionId).Distinct().ToList();
            var existingCount = await _db.AssetDefinitions
                .CountAsync(d => defIds.Contains(d.Id), ct);
            if (existingCount != defIds.Count)
                throw new DomainException("ASSET_DEF_NOT_FOUND", "One or more asset definitions not found.");

            var template = new CompositeTemplate(
                request.Name,
                request.Items.Select(i => new CompositeTemplateItemData
                {
                    AssetDefinitionId = i.AssetDefinitionId,
                    RelX = i.RelX, RelY = i.RelY,
                    Width = i.Width, Height = i.Height,
                    Rotation = i.Rotation,
                }).ToList());

            _db.CompositeTemplates.Add(template);
            await _db.SaveChangesAsync(ct);
            return template.Id;
        }
    }
}
