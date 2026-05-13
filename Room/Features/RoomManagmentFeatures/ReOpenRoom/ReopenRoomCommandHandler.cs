using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.RoomManagment.ReOpenRoom
{
    public sealed record ReopenRoomCommand(Guid userId, Guid RoomId) : IRequest;
    public sealed class ReopenRoomCommandHandler(RoomDesignDbContext _db, IFacultyFacade _facultyFacade)
        : IRequestHandler<ReopenRoomCommand>
    {
        public async Task Handle(ReopenRoomCommand request, CancellationToken cancellationToken)
        {
            var room = await _db.Rooms
                .FirstOrDefaultAsync(x => x.Id == request.RoomId, cancellationToken)
                ?? throw new InvalidOperationException($"Couldn't find a room with the Id {request.RoomId}");

            if (!await _facultyFacade.IsAssetManagerOfFaculty(request.userId, room.FacultyId))
                throw new UnauthorizedAccessException("Asset Manager Doesn't belong to this faculty");

            room.Reopen();
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}