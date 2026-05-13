using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Presistence;
using UAMS.Room.ViewDtos;

namespace UAMS.Room.Features.TicketFeatures.MarkFixed
{
    public sealed record MarkFixedCommand(Guid ticketId, Guid MaintainerId) : IRequest;
    internal class MarkFixedCommandHandler(RoomDesignDbContext _db)
        : IRequestHandler<MarkFixedCommand>
    {
        public async Task Handle(MarkFixedCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _db.Tickets.FirstOrDefaultAsync(x => x.Id == request.ticketId)
                ?? throw new InvalidOperationException("couldn't find the ticket");

            var room = await _db.Rooms
                .Include(x => x.Layout)
                .FirstOrDefaultAsync(x => x.Id == ticket.RoomId);

            if (ticket.MaintainerId != request.MaintainerId)
                throw new UnauthorizedAccessException("Only assigned maintainer can do this");

            ticket.MarkFixed(request.MaintainerId);
            var condition = ticket.GetConditionForCurrentStatus();
            if (condition != null)
                room.Layout.UpdateAssetCondition(ticket.PlacedAssetId, condition.Value);
        }
    }
}