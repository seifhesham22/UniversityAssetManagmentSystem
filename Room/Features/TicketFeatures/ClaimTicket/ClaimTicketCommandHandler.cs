using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.TicketFeatures.ClaimTicket
{
    public sealed record ClaimTicketCommand(
        Guid TicketId,
        Guid MaintainerUserId) : IRequest;

    internal sealed class ClaimTicketCommandHandler(
        RoomDesignDbContext _db,
        ICampusFacade _campusFacade)
        : IRequestHandler<ClaimTicketCommand>
    {
        public async Task Handle(ClaimTicketCommand request, CancellationToken ct)
        {
            var maintainerId = await _campusFacade.GetMaintainerIdByUserIdAsync(request.MaintainerUserId, ct)
                ?? throw new DomainException("MAINTAINER_NOT_FOUND", "Maintainer profile not found.");

            var ticket = await _db.Tickets
                .Include(t => t.TicketNotes)
                .FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
                ?? throw new DomainException("TICKET_NOT_FOUND", "Ticket not found.");

            if (!ticket.DepartmentId.HasValue)
                throw new DomainException("TICKET_NO_DEPARTMENT",
                    "This ticket has not been routed to any department yet.");

            var isInDept = await _campusFacade.IsMaintainerInDepartmentAsync(
                maintainerId, ticket.DepartmentId.Value, ct);

            if (!isInDept)
                throw new DomainException("UNAUTHORIZED",
                    "You can only claim tickets routed to your department.");

            ticket.ClaimByMaintainer(maintainerId);

            await _db.SaveChangesAsync(ct);
        }
    }
}
