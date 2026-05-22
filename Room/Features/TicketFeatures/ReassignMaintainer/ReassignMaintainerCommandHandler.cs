using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;
using UAMS.Room.VkBot;

namespace UAMS.Room.Features.TicketFeatures.ReassignMaintainer
{
    public sealed record ReassignMaintainerCommand(
        Guid TicketId,
        Guid DeptManagerUserId,
        Guid NewMaintainerId) : IRequest;

    internal sealed class ReassignMaintainerCommandHandler(
        RoomDesignDbContext _db,
        ICampusFacade _campusFacade,
        IVkBotService _vkBot)
        : IRequestHandler<ReassignMaintainerCommand>
    {
        public async Task Handle(ReassignMaintainerCommand request, CancellationToken ct)
        {
            var departmentId = await _campusFacade.GetDepartmentManagerDepartmentIdAsync(
                request.DeptManagerUserId, ct)
                ?? throw new DomainException("DEPT_MANAGER_NOT_FOUND", "Department manager profile not found.");

            var ticket = await _db.Tickets
                .Include(t => t.TicketNotes)
                .FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
                ?? throw new DomainException("TICKET_NOT_FOUND", "Ticket not found.");

            if (ticket.DepartmentId != departmentId)
                throw new DomainException("TICKET_WRONG_DEPARTMENT",
                    "This ticket is not routed to your department.");

            if (!await _campusFacade.IsMaintainerInDepartmentAsync(request.NewMaintainerId, departmentId, ct))
                throw new DomainException("MAINTAINER_NOT_IN_DEPT",
                    "The selected maintainer does not belong to your department.");

            var previousMaintainerId = ticket.MaintainerId;
            ticket.ReassignMaintainer(request.NewMaintainerId, request.DeptManagerUserId);
            await _db.SaveChangesAsync(ct);

            // Resolve shared context for notifications
            var room = await _db.Rooms.AsNoTracking().Include(r => r.Layout)
                .FirstOrDefaultAsync(r => r.Id == ticket.RoomId, ct);
            var assetName    = room?.Layout.FindAsset(ticket.PlacedAssetId)?.AssetName ?? "Unknown";
            var roomName     = room?.Name ?? "Unknown";
            var buildingInfo = room != null ? await _campusFacade.GetBuildingInfoAsync(room.BuildingId, ct) : null;
            var facultyName  = room != null ? await _campusFacade.GetFacultyNameAsync(room.FacultyId, ct) ?? "Unknown" : "Unknown";
            var amName       = room != null ? await _campusFacade.GetAssetManagerNameByFacultyIdAsync(room.FacultyId, ct) : null;

            // Resolve note author names for history
            var authorIds = ticket.TicketNotes.Select(n => n.AuthorId).Distinct().ToList();
            var authorNames = authorIds.Count > 0
                ? await _campusFacade.GetNoteAuthorNamesAsync(authorIds, ct)
                : new Dictionary<Guid, string>();

            var noteHistory = ticket.TicketNotes
                .OrderBy(n => n.CreatedAtUtc)
                .Select(n => (
                    Author:  authorNames.GetValueOrDefault(n.AuthorId, "Unknown"),
                    Content: n.Content,
                    At:      n.CreatedAtUtc));

            // Notify new maintainer with full context + history
            var newVkId = await _campusFacade.GetMaintainerVkIdAsync(request.NewMaintainerId, ct);
            if (!string.IsNullOrEmpty(newVkId))
            {
                var message  = VkNotifications.Reassigned(ticket.Id, assetName, roomName,
                    buildingInfo?.Name ?? "Unknown", buildingInfo?.Address, facultyName, amName, noteHistory);
                var keyboard = VkKeyboard.ForStatus(ticket.Status);
                var sent     = await _vkBot.SendMessageAsync(newVkId, message, keyboard, ct);
                if (sent) ticket.SetVkNotificationSent(newVkId);
                else      ticket.SetVkNotificationFailed(newVkId);
                await _db.SaveChangesAsync(ct);
            }

            // Notify previous maintainer: ticket taken away + full history
            if (previousMaintainerId.HasValue && previousMaintainerId.Value != request.NewMaintainerId)
            {
                var prevVkId = await _campusFacade.GetMaintainerVkIdAsync(previousMaintainerId.Value, ct);
                if (!string.IsNullOrEmpty(prevVkId))
                {
                    var msg = VkNotifications.PreviousMaintainerUnassigned(ticket.Id, assetName, noteHistory);
                    // Remove keyboard from previous maintainer's chat
                    await _vkBot.SendMessageAsync(prevVkId, msg, VkKeyboard.Empty(), ct);
                }
            }
        }
    }
}
