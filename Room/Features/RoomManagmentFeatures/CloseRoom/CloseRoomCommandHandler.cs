using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.RoomManagment.CloseRoom
{
    public sealed record CloseRoomCommand(Guid userId, Guid RoomId, string Reason) : IRequest;
    public sealed class CloseRoomCommandHandler(
        RoomDesignDbContext _db,
        IFacultyFacade _facultyFacade)
        : IRequestHandler<CloseRoomCommand>
    {
        public async Task Handle(CloseRoomCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Reason))
                throw new ArgumentNullException(
                    "the closure reason can't be empty");

            var room = await _db.Rooms
                .FirstOrDefaultAsync(x => x.Id == request.RoomId, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Couldn't find a room with the Id: {request.RoomId}");

            if (!await _facultyFacade.IsAssetManagerOfFaculty(request.userId, room.FacultyId))
                throw new UnauthorizedAccessException("this asset manager is not assosiated to this room");

            room.Close(request.Reason);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}