using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.TicketFeatures.SubmitInspectionResult
{
    public sealed record SubmitInspectionResultCommand(
        Guid TicketId,
        Guid MaintainerUserId,
        bool IsRepairable,
        string Notes) : IRequest;

    internal sealed class SubmitInspectionResultCommandHandler(
        RoomDesignDbContext _db,
        ICampusFacade _campusFacade)
        : IRequestHandler<SubmitInspectionResultCommand>
    {
        public async Task Handle(SubmitInspectionResultCommand request, CancellationToken ct)
        {
            var maintainerId = await _campusFacade.GetMaintainerIdByUserIdAsync(request.MaintainerUserId, ct)
                ?? throw new DomainException("MAINTAINER_NOT_FOUND", "Maintainer profile not found.");

            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
                ?? throw new DomainException("TICKET_NOT_FOUND", "Ticket not found.");

            var room = await _db.Rooms.Include(r => r.Layout)
                .FirstOrDefaultAsync(r => r.Id == ticket.RoomId, ct)
                ?? throw new DomainException("ROOM_NOT_FOUND", "Room not found.");

            ticket.SubmitInspectionResult(maintainerId, request.IsRepairable, request.Notes);

            var condition = ticket.GetConditionForCurrentStatus();
            if (condition != null)
                room.Layout.UpdateAssetCondition(ticket.PlacedAssetId, condition.Value);

            await _db.SaveChangesAsync(ct);
        }
    }
}
