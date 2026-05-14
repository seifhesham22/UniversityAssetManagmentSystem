using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.LayoutFeatures.UpdateChecklist
{
    public sealed record UpdatePlacedAssetChecklistCommand(
        Guid ChecklistId,
        Guid ChecklistItemId,
        bool IsChecked,
        Guid UserId) : IRequest;

    internal sealed class UpdatePlacedAssetChecklistCommandHandler(RoomDesignDbContext _db)
        : IRequestHandler<UpdatePlacedAssetChecklistCommand>
    {
        public async Task Handle(UpdatePlacedAssetChecklistCommand request, CancellationToken ct)
        {
            var checklist = await _db.PlacedAssetCheckLists
                .Include(c => c.Entries)
                    .ThenInclude(e => e.ChecklistItem)
                .FirstOrDefaultAsync(c => c.Id == request.ChecklistId, ct)
                ?? throw new DomainException("CHECKLIST_NOT_FOUND", "Checklist not found.");

            if (request.IsChecked)
                checklist.Check(request.ChecklistItemId, request.UserId);
            else
                checklist.Uncheck(request.ChecklistItemId);

            await _db.SaveChangesAsync(ct);
        }
    }
}
