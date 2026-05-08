using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.RoomManagment.ReOpenRoom
{
    public sealed record ReopenRoomCommand(Guid RoomId) : IRequest;
    public sealed class ReopenRoomCommandHandler(RoomDesignDbContext _db)
        : IRequestHandler<ReopenRoomCommand>
    {
        public async Task Handle(ReopenRoomCommand request, CancellationToken cancellationToken)
        {
            var room = await _db.Rooms
                .FirstOrDefaultAsync(x => x.Id == request.RoomId, cancellationToken)
                ?? throw new InvalidOperationException($"Couldn't find a room with the Id {request.RoomId}");

            room.Reopen();
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}