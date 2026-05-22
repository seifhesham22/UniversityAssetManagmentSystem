using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;
using UAMS.Room.VkBot;

namespace UAMS.Room.Features.TicketFeatures.SendForFix
{
    public sealed record SendForFixCommand(
        Guid TicketId,
        Guid UserId,
        Guid DepartmentId,
        string? Note) : IRequest;

    internal sealed class SendForFixCommandHandler(
        RoomDesignDbContext _db,
        IFacultyFacade _facultyFacade,
        ICampusFacade _campusFacade,
        IVkBotService _vkBot)
        : IRequestHandler<SendForFixCommand>
    {
        public async Task Handle(SendForFixCommand request, CancellationToken ct)
        {
            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
                ?? throw new DomainException("TICKET_NOT_FOUND", "Ticket not found.");

            var isManager = await _facultyFacade.IsAssetManagerOfFaculty(request.UserId, ticket.FacultyId);
            if (!isManager)
                throw new DomainException("UNAUTHORIZED", "Only the asset manager of this faculty can manage tickets.");

            if (!await _campusFacade.DepartmentExistsAsync(request.DepartmentId, ct))
                throw new DomainException("DEPT_NOT_FOUND", "Department not found.");

            var room = await _db.Rooms.Include(r => r.Layout)
                .FirstOrDefaultAsync(r => r.Id == ticket.RoomId, ct)
                ?? throw new DomainException("ROOM_NOT_FOUND", "Room not found.");

            ticket.SendForFix(request.UserId, request.DepartmentId, request.Note);

            var condition = ticket.GetConditionForCurrentStatus();
            if (condition != null)
                room.Layout.UpdateAssetCondition(ticket.PlacedAssetId, condition.Value);

            await _db.SaveChangesAsync(ct);

            // Push updated keyboard to the assigned maintainer so they see the fix actions immediately.
            if (ticket.MaintainerId.HasValue)
                await NotifyMaintainerAsync(ticket, room, ct);
        }

        private async Task NotifyMaintainerAsync(Models.Ticket ticket, Models.Room room, CancellationToken ct)
        {
            var vkId = await _campusFacade.GetMaintainerVkIdAsync(ticket.MaintainerId!.Value, ct);
            if (string.IsNullOrEmpty(vkId)) return;

            var buildingInfo = await _campusFacade.GetBuildingInfoAsync(room.BuildingId, ct);
            var facultyName  = await _campusFacade.GetFacultyNameAsync(room.FacultyId, ct) ?? "Unknown";
            var amName       = await _campusFacade.GetAssetManagerNameByFacultyIdAsync(room.FacultyId, ct);
            var assetName    = room.Layout.FindAsset(ticket.PlacedAssetId)?.AssetName ?? "Unknown";

            var message  = VkNotifications.TicketAssigned(ticket.Id, assetName, room.Name,
                buildingInfo?.Name ?? "Unknown", buildingInfo?.Address, facultyName, amName);
            var keyboard = VkKeyboard.ForStatus(ticket.Status);
            await _vkBot.SendMessageAsync(vkId, message, keyboard, ct);
        }
    }
}
