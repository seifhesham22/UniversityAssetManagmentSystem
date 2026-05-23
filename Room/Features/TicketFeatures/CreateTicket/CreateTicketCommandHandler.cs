using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UAMS.Room.Facades;
using UAMS.Room.Models;
using UAMS.Room.Models.Enums;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.TicketFeatures.CreateTicket
{
    public sealed record ReportTicketCommand(
    Guid PlacedAssetId, Guid RoomId, Guid FacultyId,
    Guid ReportedByUserId, string? Description) : IRequest<Guid>;
    internal sealed class CreateTicketCommandHandler(
        RoomDesignDbContext _db,
        IFacultyFacade _facultyFacade)
        : IRequestHandler<ReportTicketCommand, Guid>
    {
        public async Task<Guid> Handle(ReportTicketCommand request, CancellationToken cancellationToken)
        {
            var room = await _db.Rooms
                .Include(x => x.Layout)
                .FirstOrDefaultAsync(x => x.Id == request.RoomId && x.FacultyId == request.FacultyId, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Couldn't find a room with this id: {request.RoomId}");

            if(room.FacultyId != request.FacultyId)
                throw new InvalidOperationException(
                    "the requested Room doesn't belong to the requested Faculty");

            var hasOpen = await _db.Tickets.AnyAsync(t =>
            t.PlacedAssetId == request.PlacedAssetId
            && t.Status != TicketStatus.Closed
            && t.Status != TicketStatus.ConfirmedFixed
            && t.Status != TicketStatus.EscalatedExternally
            && t.Status != TicketStatus.Irreparable, cancellationToken);

            if (hasOpen)
                throw new InvalidOperationException("ticket already there can't duplicate");

            if (!room.Layout.HasAsset(request.PlacedAssetId))
                throw new InvalidOperationException(
                    $"room doesn't have a placed asset with the Id: {request.PlacedAssetId}");

            var facultyExist = await _facultyFacade.ExistsAsync(request.FacultyId,cancellationToken);

            var IsMemeberOfFaculty = await _facultyFacade
                .UserBelongsToFaculty(
                request.ReportedByUserId,
                request.FacultyId,
                cancellationToken);

            if (!IsMemeberOfFaculty)
                throw new UnauthorizedAccessException(
                    "the reporter must be a member of the faculty he is trying to act on");

            if (!facultyExist)
                throw new InvalidOperationException($"couldn't find a faculty with the Id: {request.FacultyId}");

            var ticket = new Ticket(
                request.PlacedAssetId,
                room.Id,
                request.FacultyId,
                request.ReportedByUserId);

            if (!string.IsNullOrWhiteSpace(request.Description))
                ticket.AddNote(request.ReportedByUserId, request.Description);

            room.Layout.UpdateAssetCondition(
                request.PlacedAssetId,
                Shared.Enums.PlacedAssetCondition.Reported);

            _db.Add(ticket);
            await _db.SaveChangesAsync(cancellationToken);
            return ticket.Id;
        }
    }
}