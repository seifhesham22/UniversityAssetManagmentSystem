using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;
using UAMS.Room.VkBot;

namespace UAMS.Room.Features.TicketFeatures.AssignMaintainer
{
    public sealed record AssignMaintainerCommand(
        Guid TicketId,
        Guid DeptManagerUserId,
        Guid MaintainerId) : IRequest;

    internal sealed class AssignMaintainerCommandHandler(
        RoomDesignDbContext _db,
        ICampusFacade _campusFacade,
        IVkBotService _vkBot)
        : IRequestHandler<AssignMaintainerCommand>
    {
        public async Task Handle(AssignMaintainerCommand request, CancellationToken ct)
        {
            var departmentId = await _campusFacade.GetDepartmentManagerDepartmentIdAsync(request.DeptManagerUserId, ct)
                ?? throw new DomainException("DEPT_MANAGER_NOT_FOUND", "Department manager profile not found.");

            var ticket = await _db.Tickets
                .Include(t => t.TicketNotes)
                .FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
                ?? throw new DomainException("TICKET_NOT_FOUND", "Ticket not found.");

            if (ticket.DepartmentId != departmentId)
                throw new DomainException("TICKET_WRONG_DEPARTMENT",
                    "This ticket is not routed to your department.");

            if (!await _campusFacade.IsMaintainerInDepartmentAsync(request.MaintainerId, departmentId, ct))
                throw new DomainException("MAINTAINER_NOT_IN_DEPT",
                    "The selected maintainer does not belong to your department.");

            ticket.AssignToMaintainer(request.MaintainerId);
            await _db.SaveChangesAsync(ct);

            // Send VK notification if maintainer has a VK ID
            var vkId = await _campusFacade.GetMaintainerVkIdAsync(request.MaintainerId, ct);
            if (!string.IsNullOrEmpty(vkId))
            {
                var (message, keyboard) = await BuildNotification(ticket, ct);
                var sent = await _vkBot.SendMessageAsync(vkId, message, keyboard, ct);
                if (sent) ticket.SetVkNotificationSent(vkId);
                else      ticket.SetVkNotificationFailed(vkId);
                await _db.SaveChangesAsync(ct);
            }
        }

        internal async Task<(string Message, string? Keyboard)> BuildNotification(
            Models.Ticket ticket, CancellationToken ct)
        {
            var room = await _db.Rooms.AsNoTracking().Include(r => r.Layout)
                .FirstOrDefaultAsync(r => r.Id == ticket.RoomId, ct);

            var assetName    = room?.Layout.FindAsset(ticket.PlacedAssetId)?.AssetName ?? "Unknown";
            var roomName     = room?.Name ?? "Unknown";
            var buildingInfo = room != null ? await _campusFacade.GetBuildingInfoAsync(room.BuildingId, ct) : null;
            var facultyName  = room != null ? await _campusFacade.GetFacultyNameAsync(room.FacultyId, ct) ?? "Unknown" : "Unknown";
            var amName       = room != null ? await _campusFacade.GetAssetManagerNameByFacultyIdAsync(room.FacultyId, ct) : null;

            IEnumerable<(string, string, DateTime)>? noteHistory = null;
            if (ticket.TicketNotes.Count > 0)
            {
                var authorIds   = ticket.TicketNotes.Select(n => n.AuthorId).Distinct().ToList();
                var authorNames = await _campusFacade.GetNoteAuthorNamesAsync(authorIds, ct);
                noteHistory = ticket.TicketNotes
                    .OrderBy(n => n.CreatedAtUtc)
                    .Select(n => (authorNames.GetValueOrDefault(n.AuthorId, "Unknown"), n.Content, n.CreatedAtUtc));
            }

            var message  = VkNotifications.TicketAssigned(ticket.Id, assetName, roomName,
                buildingInfo?.Name ?? "Unknown", buildingInfo?.Address, facultyName, amName, noteHistory);
            var keyboard = VkKeyboard.ForStatus(ticket.Status);
            return (message, keyboard);
        }
    }
}
