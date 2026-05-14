using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.TicketFeatures.SendForReplacement
{
    public sealed record SendForReplacementCommand(
        Guid TicketId,
        Guid UserId,
        Guid DepartmentId,
        string? Note) : IRequest;

    internal sealed class SendForReplacementCommandHandler(
        RoomDesignDbContext _db,
        IFacultyFacade _facultyFacade,
        ICampusFacade _campusFacade)
        : IRequestHandler<SendForReplacementCommand>
    {
        public async Task Handle(SendForReplacementCommand request, CancellationToken ct)
        {
            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
                ?? throw new DomainException("TICKET_NOT_FOUND", "Ticket not found.");

            var isManager = await _facultyFacade.IsAssetManagerOfFaculty(request.UserId, ticket.FacultyId);
            if (!isManager)
                throw new DomainException("UNAUTHORIZED", "Only the asset manager of this faculty can manage tickets.");

            if (!await _campusFacade.DepartmentExistsAsync(request.DepartmentId, ct))
                throw new DomainException("DEPT_NOT_FOUND", "Department not found.");

            var room = await _db.Rooms.Include(r => r.Layout)
                .FirstOrDefaultAsync(r => r.Id == ticket.RoomId, ct)
                ?? throw new DomainException("ROOM_NOT_FOUND", "Room not found.");

            ticket.SendForReplacement(request.UserId, request.DepartmentId, request.Note);

            var condition = ticket.GetConditionForCurrentStatus();
            if (condition != null)
                room.Layout.UpdateAssetCondition(ticket.PlacedAssetId, condition.Value);

            await _db.SaveChangesAsync(ct);
        }
    }
}
