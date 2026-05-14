using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.TicketFeatures.CloseTicket
{
    public sealed record CloseTicketCommand(Guid TicketId, Guid UserId, string? Note) : IRequest;

    internal sealed class CloseTicketCommandHandler(
        RoomDesignDbContext _db,
        IFacultyFacade _facultyFacade)
        : IRequestHandler<CloseTicketCommand>
    {
        public async Task Handle(CloseTicketCommand request, CancellationToken ct)
        {
            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
                ?? throw new DomainException("TICKET_NOT_FOUND", "Ticket not found.");

            var isManager = await _facultyFacade.IsAssetManagerOfFaculty(request.UserId, ticket.FacultyId);
            if (!isManager)
                throw new DomainException("UNAUTHORIZED", "Only the asset manager of this faculty can close tickets.");

            ticket.Close(request.UserId, request.Note);

            await _db.SaveChangesAsync(ct);
        }
    }
}
