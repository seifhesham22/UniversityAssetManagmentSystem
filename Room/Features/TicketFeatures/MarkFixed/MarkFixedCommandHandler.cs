using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.TicketFeatures.MarkFixed
{
    public sealed record MarkFixedCommand(Guid ticketId, Guid MaintainerUserId) : IRequest;

    internal sealed class MarkFixedCommandHandler(
        RoomDesignDbContext _db,
        ICampusFacade _campusFacade)
        : IRequestHandler<MarkFixedCommand>
    {
        public async Task Handle(MarkFixedCommand request, CancellationToken cancellationToken)
        {
            var maintainerId = await _campusFacade.GetMaintainerIdByUserIdAsync(request.MaintainerUserId, cancellationToken)
                ?? throw new DomainException("MAINTAINER_NOT_FOUND", "Maintainer profile not found.");

            var ticket = await _db.Tickets.FirstOrDefaultAsync(x => x.Id == request.ticketId, cancellationToken)
                ?? throw new DomainException("TICKET_NOT_FOUND", "Ticket not found.");

            var room = await _db.Rooms
                .Include(x => x.Layout)
                .FirstOrDefaultAsync(x => x.Id == ticket.RoomId, cancellationToken)
                ?? throw new DomainException("ROOM_NOT_FOUND", "Room not found.");

            ticket.MarkFixed(maintainerId);

            var condition = ticket.GetConditionForCurrentStatus();
            if (condition != null)
                room.Layout.UpdateAssetCondition(ticket.PlacedAssetId, condition.Value);

            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
