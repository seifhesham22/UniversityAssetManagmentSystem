using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Models.Enums;
using UAMS.Room.Presistence;
using UAMS.Room.ViewDtos;

namespace UAMS.Room.Features.TicketFeatures.GetDepartmentTickets
{
    public sealed record GetDepartmentTicketsQuery(
        Guid DeptManagerUserId,
        bool NeedsActionOnly = false) : IRequest<List<DeptManagerTicketItem>>;

    internal sealed class GetDepartmentTicketsQueryHandler(
        RoomDesignDbContext _db,
        ICampusFacade _campusFacade)
        : IRequestHandler<GetDepartmentTicketsQuery, List<DeptManagerTicketItem>>
    {
        public async Task<List<DeptManagerTicketItem>> Handle(GetDepartmentTicketsQuery request, CancellationToken ct)
        {
            var departmentId = await _campusFacade.GetDepartmentManagerDepartmentIdAsync(request.DeptManagerUserId, ct)
                ?? throw new DomainException("DEPT_MANAGER_NOT_FOUND", "Department manager profile not found.");

            var terminalStatuses = new[] {
                TicketStatus.Closed, TicketStatus.ConfirmedFixed, TicketStatus.EscalatedExternally
            };

            var query = _db.Tickets.AsNoTracking()
                .Include(t => t.TicketNotes)
                .Where(t => t.DepartmentId == departmentId);

            if (request.NeedsActionOnly)
                query = query.Where(t => t.MaintainerId == null && !terminalStatuses.Contains(t.Status));

            var tickets = await query.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);

            if (tickets.Count == 0)
                return [];

            var roomIds = tickets.Select(t => t.RoomId).Distinct().ToList();
            var rooms = await _db.Rooms
                .AsNoTracking()
                .Include(r => r.Layout)
                .Where(r => roomIds.Contains(r.Id))
                .ToListAsync(ct);
            var roomLookup = rooms.ToDictionary(r => r.Id);

            var facultyIds = rooms.Select(r => r.FacultyId).Distinct().ToList();
            var buildingIds = rooms.Select(r => r.BuildingId).Distinct().ToList();

            var facultyNames = new Dictionary<Guid, string?>();
            foreach (var fid in facultyIds)
                facultyNames[fid] = await _campusFacade.GetFacultyNameAsync(fid, ct);

            var buildingNames = new Dictionary<Guid, string?>();
            foreach (var bid in buildingIds)
                buildingNames[bid] = await _campusFacade.GetBuildingNameAsync(bid, ct);

            var noteAuthorIds = tickets
                .SelectMany(t => t.TicketNotes.Select(n => n.AuthorId))
                .Distinct()
                .ToList();
            var noteAuthorNames = noteAuthorIds.Count > 0
                ? await _campusFacade.GetNoteAuthorNamesAsync(noteAuthorIds, ct)
                : new Dictionary<Guid, string>();

            var maintainerIds = tickets
                .Where(t => t.MaintainerId.HasValue)
                .Select(t => t.MaintainerId!.Value)
                .Distinct()
                .ToList();

            var maintainerNames = new Dictionary<Guid, string?>();
            foreach (var mid in maintainerIds)
                maintainerNames[mid] = await _campusFacade.GetMaintainerNameAsync(mid, ct);

            return tickets.Select(t =>
            {
                roomLookup.TryGetValue(t.RoomId, out var room);
                var assetName = room?.Layout.FindAsset(t.PlacedAssetId)?.AssetName ?? "Unknown";
                var roomName = room?.Name ?? "Unknown";
                var facultyId = room?.FacultyId ?? t.FacultyId;
                var buildingId = room?.BuildingId ?? Guid.Empty;

                var notes = t.TicketNotes
                    .OrderBy(n => n.CreatedAtUtc)
                    .Select(n => new TicketNoteView(
                        n.Id, n.AuthorId,
                        noteAuthorNames.GetValueOrDefault(n.AuthorId, "Unknown"),
                        n.Content, n.CreatedAtUtc))
                    .ToList();

                return new DeptManagerTicketItem(
                    Id: t.Id,
                    AssetName: assetName,
                    RoomId: t.RoomId,
                    RoomName: roomName,
                    FacultyId: facultyId,
                    FacultyName: facultyNames.GetValueOrDefault(facultyId) ?? "Unknown",
                    BuildingName: buildingNames.GetValueOrDefault(buildingId) ?? "Unknown",
                    Status: t.Status.ToString(),
                    Decision: t.Decision.ToString(),
                    CurrentMaintainerId: t.MaintainerId,
                    CurrentMaintainerName: t.MaintainerId.HasValue
                        ? maintainerNames.GetValueOrDefault(t.MaintainerId.Value)
                        : null,
                    CreatedAtUtc: t.CreatedAt,
                    UpdatedAtUtc: t.LastUpdatedAt,
                    VkNotificationStatus: t.VkNotificationStatus.ToString(),
                    Notes: notes);
            }).ToList();
        }
    }
}
