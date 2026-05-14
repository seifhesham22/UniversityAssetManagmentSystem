using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.TicketFeatures.ConfirmFixed
{
    public sealed record ConfirmFixedCommand(Guid TicketId, Guid UserId) : IRequest;

    internal sealed class ConfirmFixedCommandHandler(RoomDesignDbContext _db)
        : IRequestHandler<ConfirmFixedCommand>
    {
        public async Task Handle(ConfirmFixedCommand request, CancellationToken ct)
        {
            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
                ?? throw new DomainException("TICKET_NOT_FOUND", "Ticket not found.");

            var room = await _db.Rooms.Include(r => r.Layout)
                .FirstOrDefaultAsync(r => r.Id == ticket.RoomId, ct)
                ?? throw new DomainException("ROOM_NOT_FOUND", "Room not found.");

            ticket.ConfirmFixed(request.UserId);

            var condition = ticket.GetConditionForCurrentStatus();
            if (condition != null)
                room.Layout.UpdateAssetCondition(ticket.PlacedAssetId, condition.Value);

            await _db.SaveChangesAsync(ct);
        }
    }
}
