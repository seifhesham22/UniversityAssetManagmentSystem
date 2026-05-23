using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Models.Enums;
using UAMS.Room.Presistence;
using UAMS.Room.ViewDtos;

namespace UAMS.Room.Features.TicketFeatures.GetFacultyTickets
{
    public sealed record GetFacultyTicketsQuery(
        Guid AssetManagerUserId,
        Guid FacultyId,
        bool NeedsActionOnly = false) : IRequest<List<TicketListItem>>;

    internal sealed class GetFacultyTicketsQueryHandler(
        RoomDesignDbContext _db,
        IFacultyFacade _facultyFacade,
        ICampusFacade _campusFacade)
        : IRequestHandler<GetFacultyTicketsQuery, List<TicketListItem>>
    {
        public async Task<List<TicketListItem>> Handle(GetFacultyTicketsQuery request, CancellationToken ct)
        {
            var isManager = await _facultyFacade.IsAssetManagerOfFaculty(request.AssetManagerUserId, request.FacultyId);
            if (!isManager)
                throw new DomainException("UNAUTHORIZED", "You are not the asset manager of this faculty.");

            var actionStatuses = new[] {
                TicketStatus.InspectionDone, TicketStatus.Irreparable,
                TicketStatus.Fixed, TicketStatus.Replaced
            };

            var query = _db.Tickets.AsNoTracking()
                .Include(t => t.TicketNotes)
                .Where(t => t.FacultyId == request.FacultyId);

            if (request.NeedsActionOnly)
                query = query.Where(t => actionStatuses.Contains(t.Status));

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

            var reporterIds = tickets.Select(t => t.ReporterId).Distinct().ToList();
            var reporterNames = await _campusFacade.GetUserNamesAsync(reporterIds, ct);

            // Collect all note author IDs for bulk name lookup
            var noteAuthorIds = tickets
                .SelectMany(t => t.TicketNotes.Select(n => n.AuthorId))
                .Distinct()
                .ToList();
            var noteAuthorInfo = noteAuthorIds.Count > 0
                ? await _campusFacade.GetNoteAuthorInfoAsync(noteAuthorIds, ct)
                : new Dictionary<Guid, (string Name, string Role)>();

            var maintainerIds = tickets
                .Where(t => t.MaintainerId.HasValue)
                .Select(t => t.MaintainerId!.Value)
                .Distinct()
                .ToList();

            var maintainerNames = new Dictionary<Guid, string?>();
            foreach (var mid in maintainerIds)
            {
                maintainerNames[mid] = await _campusFacade.GetMaintainerNameAsync(mid, ct);
            }

            var deptIds = tickets
                .Where(t => t.DepartmentId.HasValue)
                .Select(t => t.DepartmentId!.Value)
                .Distinct()
                .ToList();

            var deptNames = new Dictionary<Guid, string?>();
            foreach (var did in deptIds)
            {
                deptNames[did] = await _campusFacade.GetDepartmentNameAsync(did, ct);
            }

            return tickets.Select(t =>
            {
                roomLookup.TryGetValue(t.RoomId, out var room);
                var assetName = room?.Layout.FindAsset(t.PlacedAssetId)?.AssetName ?? "Unknown";
                var roomName = room?.Name ?? "Unknown";

                var notes = t.TicketNotes
                    .OrderBy(n => n.CreatedAtUtc)
                    .Select(n =>
                    {
                        var info = noteAuthorInfo.GetValueOrDefault(n.AuthorId, (Name: "Unknown", Role: "Unknown"));
                        var role = n.AuthorId == t.ReporterId ? "Reporter" : info.Role;
                        return new TicketNoteView(n.Id, n.AuthorId, info.Name, role, n.Content, n.CreatedAtUtc);
                    })
                    .ToList();

                return new TicketListItem(
                    Id: t.Id,
                    AssetName: assetName,
                    RoomName: roomName,
                    RoomCode: roomName,
                    FacultyId: t.FacultyId,
                    ReportedByName: reporterNames.GetValueOrDefault(t.ReporterId, "Unknown"),
                    Status: t.Status.ToString(),
                    Decision: t.Decision.ToString(),
                    AssignedToDepartmentId: t.DepartmentId,
                    DepartmentName: t.DepartmentId.HasValue
                        ? deptNames.GetValueOrDefault(t.DepartmentId.Value)
                        : null,
                    CurrentMaintainerId: t.MaintainerId,
                    CurrentMaintainerName: t.MaintainerId.HasValue
                        ? maintainerNames.GetValueOrDefault(t.MaintainerId.Value)
                        : null,
                    CreatedAtUtc: t.CreatedAt,
                    UpdatedAtUtc: t.LastUpdatedAt,
                    Notes: notes);
            }).ToList();
        }
    }
}
