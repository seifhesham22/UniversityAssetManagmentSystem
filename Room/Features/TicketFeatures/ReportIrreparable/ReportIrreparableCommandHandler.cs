using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.TicketFeatures.ReportIrreparable
{
    public sealed record ReportIrreparableCommand(
        Guid TicketId,
        Guid MaintainerUserId,
        string Reason) : IRequest;

    internal sealed class ReportIrreparableCommandHandler(
        RoomDesignDbContext _db,
        ICampusFacade _campusFacade)
        : IRequestHandler<ReportIrreparableCommand>
    {
        public async Task Handle(ReportIrreparableCommand request, CancellationToken ct)
        {
            var maintainerId = await _campusFacade.GetMaintainerIdByUserIdAsync(request.MaintainerUserId, ct)
                ?? throw new DomainException("MAINTAINER_NOT_FOUND", "Maintainer profile not found.");

            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
                ?? throw new DomainException("TICKET_NOT_FOUND", "Ticket not found.");

            var room = await _db.Rooms.Include(r => r.Layout)
                .FirstOrDefaultAsync(r => r.Id == ticket.RoomId, ct)
                ?? throw new DomainException("ROOM_NOT_FOUND", "Room not found.");

            ticket.ReportIrreparable(maintainerId, request.Reason);

            var condition = ticket.GetConditionForCurrentStatus();
            if (condition != null)
                room.Layout.UpdateAssetCondition(ticket.PlacedAssetId, condition.Value);

            await _db.SaveChangesAsync(ct);
        }
    }
}
