using MediatR;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.RoomManagment.CreateRoom
{
    public sealed record CreateRoomCommand(
        Guid FacultyId,
        Guid BuildingId,
        Guid DesignedByUserId,
        string name) : IRequest<Guid>;
    public sealed class CreateRoomCommandHandler(RoomDesignDbContext _db, IFacultyFacade _facultyFacade)
        : IRequestHandler<CreateRoomCommand, Guid>
    {
        public async Task<Guid> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
        {
            if (!await _facultyFacade.ExistsAsync(request.FacultyId, cancellationToken))
                throw new InvalidOperationException(
                    $"Couldn't find a faculty with the Id {request.FacultyId}");

            if (!await _facultyFacade.IsBuildingLinkedAsync(request.FacultyId, request.BuildingId, cancellationToken))
                throw new InvalidOperationException(
                    $"Building not linked to this room");

            if (await _db.Rooms.AnyAsync(x => x.Name == request.name))
                throw new InvalidOperationException(
                    $"Room with this name: {request.name} already exists");

            var newRoom = new UAMS.Room.Models.Room(
                request.name,
                request.BuildingId,
                request.FacultyId,
                request.DesignedByUserId);

            await _db.Rooms.AddAsync(newRoom);
            await _db.SaveChangesAsync(cancellationToken);

            return newRoom.Id;
        }
    }
}