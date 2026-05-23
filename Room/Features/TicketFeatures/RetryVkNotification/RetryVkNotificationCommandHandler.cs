using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;
using UAMS.Room.VkBot;

namespace UAMS.Room.Features.TicketFeatures.RetryVkNotification
{
    public sealed record RetryVkNotificationCommand(Guid TicketId, Guid DeptManagerUserId) : IRequest<bool>;

    internal sealed class RetryVkNotificationCommandHandler(
        RoomDesignDbContext _db,
        ICampusFacade _campusFacade,
        IVkBotService _vkBot)
        : IRequestHandler<RetryVkNotificationCommand, bool>
    {
        public async Task<bool> Handle(RetryVkNotificationCommand request, CancellationToken ct)
        {
            var deptId = await _campusFacade.GetDepartmentManagerDepartmentIdAsync(request.DeptManagerUserId, ct)
                ?? throw new DomainException("DEPT_MANAGER_NOT_FOUND", "Department manager not found.");

            var ticket = await _db.Tickets
                .Include(t => t.TicketNotes)
                .FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
                ?? throw new DomainException("TICKET_NOT_FOUND", "Ticket not found.");

            if (ticket.DepartmentId != deptId)
                throw new DomainException("UNAUTHORIZED", "Ticket does not belong to your department.");

            if (ticket.MaintainerId == null)
                throw new DomainException("NO_MAINTAINER", "No maintainer assigned to this ticket.");

            var vkId = ticket.AssignedMaintainerVkId
                ?? await _campusFacade.GetMaintainerVkIdAsync(ticket.MaintainerId.Value, ct);

            if (string.IsNullOrEmpty(vkId))
                throw new DomainException("NO_VK_ID", "This maintainer has no VK ID configured.");

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

            var sent = await _vkBot.SendMessageAsync(vkId, message, keyboard, ct);
            if (sent) ticket.SetVkNotificationSent(vkId);
            else      ticket.SetVkNotificationFailed(vkId);

            await _db.SaveChangesAsync(ct);
            return sent;
        }
    }
}
