using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Models;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.LayoutFeatures
{
    public sealed record SaveLayoutCommand(
        Guid RoomId,
        Guid UserId,
        List<PlacedAssetDto> PlacedAssets) : IRequest;

    public sealed record PlacedAssetDto(
        Guid Id,
        Guid AssetDefinitionId,
        string AssetName,
        float X, float Y,
        float W, float H,
        float Rotation,
        Guid? GroupId,
        string? GroupLabel,
        Guid? CanvasRoomId);

    internal sealed class SaveLayoutHandler(
        RoomDesignDbContext db,
        IFacultyFacade _facultyFacade)
        : IRequestHandler<SaveLayoutCommand>
    {
        public async Task Handle(SaveLayoutCommand cmd, CancellationToken ct)
        {
            var room = await db.Rooms
                .Include(r => r.Layout)
                .FirstOrDefaultAsync(r => r.Id == cmd.RoomId, ct)
                ?? throw new DomainException("ROOM_NOT_FOUND", "Room not found.");

            var isManager = await _facultyFacade.IsAssetManagerOfFaculty(cmd.UserId, room.FacultyId);
            if (!isManager)
                throw new DomainException("UNAUTHORIZED",
                    "Only the asset manager of this faculty can modify the room layout.");

            var requestedDefIds = cmd.PlacedAssets
                .Select(a => a.AssetDefinitionId)
                .Distinct()
                .ToList();

            var assetDefs = await db.AssetDefinitions
                .Include(ad => ad.ChecklistTemplate)
                .Where(ad => requestedDefIds.Contains(ad.Id))
                .ToListAsync(ct);

            var existingDefIds = assetDefs.Select(ad => ad.Id).ToHashSet();
            var missing = requestedDefIds.Where(id => !existingDefIds.Contains(id)).ToList();
            if (missing.Count > 0)
                throw new DomainException("ASSET_DEF_NOT_FOUND",
                    $"Unknown AssetDefinitionIds: {string.Join(", ", missing)}");

            var incoming = cmd.PlacedAssets.Select(a => new PlacedAssetEntry
            {
                Id = a.Id,
                AssetDefinitionId = a.AssetDefinitionId,
                AssetName = a.AssetName,
                X = a.X,
                Y = a.Y,
                Width = a.W,
                Height = a.H,
                Rotation = a.Rotation,
                GroupId = a.GroupId,
                GroupLabel = a.GroupLabel,
                CanvasRoomId = a.CanvasRoomId,
            }).ToList();

            var diff = room.Layout.ApplySnapshot(incoming, cmd.UserId);

            if (diff.RemovedPlacedAssetIds.Count > 0)
            {
                var checklistsToRemove = await db.PlacedAssetCheckLists
                    .Where(c => diff.RemovedPlacedAssetIds.Contains(c.PlacedAssetId))
                    .ToListAsync(ct);

                db.PlacedAssetCheckLists.RemoveRange(checklistsToRemove);
            }

            if (diff.AddedPlacedAssetIds.Count > 0)
            {
                var defLookup = assetDefs.ToDictionary(ad => ad.Id);
                var studyYear = CurrentStudyYear();

                foreach (var addedAsset in incoming.Where(a => diff.AddedPlacedAssetIds.Contains(a.Id)))
                {
                    if (!defLookup.TryGetValue(addedAsset.AssetDefinitionId, out var def))
                        continue;

                    if (def.ChecklistTemplate.Count == 0)
                        continue;

                    var checklist = new PlacedAssetChecklist(addedAsset.Id, studyYear, def.ChecklistTemplate);
                    db.PlacedAssetCheckLists.Add(checklist);
                }
            }

            await db.SaveChangesAsync(ct);
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
