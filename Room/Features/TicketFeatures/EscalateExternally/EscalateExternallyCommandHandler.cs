using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.TicketFeatures.EscalateExternally
{
    public sealed record EscalateExternallyCommand(
        Guid ticketId,
        Guid userId,
        string? note) : IRequest;
    internal sealed class EscalateExternallyCommandHandler(RoomDesignDbContext _db)
    : IRequestHandler<EscalateExternallyCommand>
    {
        public async Task Handle(EscalateExternallyCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _db.Tickets.FirstOrDefaultAsync(x => x.Id == request.ticketId)
                ?? throw new InvalidOperationException("couldn't find the ticket");

            var room = await _db.Rooms
                .Include(x => x.Layout)
                .FirstOrDefaultAsync(x => x.Id == ticket.RoomId);

            if (room.DesignedByUserId != request.userId)
                throw new UnauthorizedAccessException("Only asset manager can do this");

            ticket.EsculateExternally(request.userId, request.note);
            var condition = ticket.GetConditionForCurrentStatus();
            if (condition != null)
                room.Layout.UpdateAssetCondition(ticket.PlacedAssetId, condition.Value);

            await _db.SaveChangesAsync();
        }
    }
}