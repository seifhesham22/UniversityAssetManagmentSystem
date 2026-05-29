using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.CompositeTemplateFeatures.DeleteCompositeTemplate
{
    public sealed record DeleteCompositeTemplateCommand(Guid Id) : IRequest;

    internal sealed class DeleteCompositeTemplateCommandHandler(RoomDesignDbContext _db)
        : IRequestHandler<DeleteCompositeTemplateCommand>
    {
        public async Task Handle(DeleteCompositeTemplateCommand request, CancellationToken ct)
        {
            var template = await _db.CompositeTemplates
                .FirstOrDefaultAsync(t => t.Id == request.Id, ct)
                ?? throw new DomainException("COMPOSITE_NOT_FOUND", "Composite template not found.");

            _db.CompositeTemplates.Remove(template);
            await _db.SaveChangesAsync(ct);
        }
    }
}
