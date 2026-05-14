using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Presistence;
using UAMS.Room.ViewDtos;

namespace UAMS.Room.Features.LayoutFeatures.GetChecklistForPlacedAsset
{
    public sealed record GetChecklistForPlacedAssetQuery(
        Guid PlacedAssetId,
        string? StudyYear = null) : IRequest<ChecklistView>;

    internal sealed class GetChecklistForPlacedAssetQueryHandler(RoomDesignDbContext _db)
        : IRequestHandler<GetChecklistForPlacedAssetQuery, ChecklistView>
    {
        public async Task<ChecklistView> Handle(GetChecklistForPlacedAssetQuery request, CancellationToken ct)
        {
            var studyYear = request.StudyYear ?? CurrentStudyYear();

            var checklist = await _db.PlacedAssetCheckLists
                .Include(c => c.Entries)
                    .ThenInclude(e => e.ChecklistItem)
                .FirstOrDefaultAsync(c =>
                    c.PlacedAssetId == request.PlacedAssetId &&
                    c.StudyYear == studyYear, ct)
                ?? throw new DomainException("CHECKLIST_NOT_FOUND",
                    $"No checklist found for this asset in study year {studyYear}.");

            return new ChecklistView(
                Id: checklist.Id,
                PlacedAssetId: checklist.PlacedAssetId,
                StudyYear: checklist.StudyYear,
                CheckedCount: checklist.CheckedCount,
                TotalCount: checklist.TotalCount,
                Entries: checklist.Entries.Select(e => new ChecklistEntryView(
                    Id: e.Id,
                    ChecklistItemId: e.ChecklistItem.Id,
                    Description: e.ChecklistItem.Description,
                    IsChecked: e.IsChecked,
                    CheckedByUserId: e.CheckedByUserId,
                    CheckedAtUtc: e.CheckedAtUtc)).ToList());
        }

        private static string CurrentStudyYear()
        {
            var now = DateTime.UtcNow;
            return now.Month >= 9
                ? $"{now.Year}-{now.Year + 1}"
                : $"{now.Year - 1}-{now.Year}";
        }
    }
}
