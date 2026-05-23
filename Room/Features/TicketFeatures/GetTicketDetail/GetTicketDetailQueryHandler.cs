using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;
using UAMS.Room.ViewDtos;

namespace UAMS.Room.Features.TicketFeatures.GetTicketDetail
{
    public sealed record GetTicketDetailQuery(Guid TicketId) : IRequest<TicketDetailView>;

    internal sealed class GetTicketDetailQueryHandler(
        RoomDesignDbContext _db,
        ICampusFacade _campusFacade)
        : IRequestHandler<GetTicketDetailQuery, TicketDetailView>
    {
        public async Task<TicketDetailView> Handle(GetTicketDetailQuery request, CancellationToken ct)
        {
            var ticket = await _db.Tickets
                .AsNoTracking()
                .Include(t => t.TicketNotes)
                .FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
                ?? throw new DomainException("TICKET_NOT_FOUND", "Ticket not found.");

            var room = await _db.Rooms
                .AsNoTracking()
                .Include(r => r.Layout)
                .FirstOrDefaultAsync(r => r.Id == ticket.RoomId, ct);

            var assetName = room?.Layout.FindAsset(ticket.PlacedAssetId)?.AssetName ?? "Unknown";
            var roomName = room?.Name ?? "Unknown";

            var userIds = new HashSet<Guid> { ticket.ReporterId };
            if (ticket.ConfirmedByUserId.HasValue) userIds.Add(ticket.ConfirmedByUserId.Value);
            var userNames = await _campusFacade.GetUserNamesAsync(userIds, ct);

            var noteAuthorIds = ticket.TicketNotes.Select(n => n.AuthorId).Distinct().ToList();
            var noteAuthorInfo = noteAuthorIds.Count > 0
                ? await _campusFacade.GetNoteAuthorInfoAsync(noteAuthorIds, ct)
                : new Dictionary<Guid, (string Name, string Role)>();

            string? deptName = null;
            if (ticket.DepartmentId.HasValue)
                deptName = await _campusFacade.GetDepartmentNameAsync(ticket.DepartmentId.Value, ct);

            string? maintainerName = null;
            if (ticket.MaintainerId.HasValue)
                maintainerName = await _campusFacade.GetMaintainerNameAsync(ticket.MaintainerId.Value, ct);

            string? confirmedByName = null;
            if (ticket.ConfirmedByUserId.HasValue)
                confirmedByName = userNames.GetValueOrDefault(ticket.ConfirmedByUserId.Value);

            var notes = ticket.TicketNotes
                .OrderBy(n => n.CreatedAtUtc)
                .Select(n =>
                {
                    var info = noteAuthorInfo.GetValueOrDefault(n.AuthorId, (Name: "Unknown", Role: "Unknown"));
                    var role = n.AuthorId == ticket.ReporterId ? "Reporter" : info.Role;
                    return new TicketNoteView(n.Id, n.AuthorId, info.Name, role, n.Content, n.CreatedAtUtc);
                })
                .ToList();

            return new TicketDetailView(
                Id: ticket.Id,
                PlacedAssetId: ticket.PlacedAssetId,
                AssetName: assetName,
                RoomId: ticket.RoomId,
                RoomName: roomName,
                RoomCode: roomName,
                FacultyId: ticket.FacultyId,
                ReportedByUserId: ticket.ReporterId,
                ReportedByName: userNames.GetValueOrDefault(ticket.ReporterId, "Unknown"),
                Status: ticket.Status.ToString(),
                Decision: ticket.Decision.ToString(),
                AssignedToDepartmentId: ticket.DepartmentId,
                DepartmentName: deptName,
                CurrentMaintainerId: ticket.MaintainerId,
                CurrentMaintainerName: maintainerName,
                ConfirmedByUserId: ticket.ConfirmedByUserId,
                ConfirmedByName: confirmedByName,
                CreatedAtUtc: ticket.CreatedAt,
                UpdatedAtUtc: ticket.LastUpdatedAt,
                Notes: notes);
        }
    }
}
