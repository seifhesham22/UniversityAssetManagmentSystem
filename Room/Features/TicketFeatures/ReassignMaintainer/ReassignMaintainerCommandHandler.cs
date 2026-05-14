using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.TicketFeatures.ReassignMaintainer
{
    public sealed record ReassignMaintainerCommand(
        Guid TicketId,
        Guid DeptManagerUserId,
        Guid NewMaintainerId) : IRequest;

    internal sealed class ReassignMaintainerCommandHandler(
        RoomDesignDbContext _db,
        ICampusFacade _campusFacade)
        : IRequestHandler<ReassignMaintainerCommand>
    {
        public async Task Handle(ReassignMaintainerCommand request, CancellationToken ct)
        {
            var departmentId = await _campusFacade.GetDepartmentManagerDepartmentIdAsync(
                request.DeptManagerUserId, ct)
                ?? throw new DomainException("DEPT_MANAGER_NOT_FOUND", "Department manager profile not found.");

            var ticket = await _db.Tickets
                .Include(t => t.TicketNotes)
                .FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
                ?? throw new DomainException("TICKET_NOT_FOUND", "Ticket not found.");

            if (ticket.DepartmentId != departmentId)
                throw new DomainException("TICKET_WRONG_DEPARTMENT",
                    "This ticket is not routed to your department.");

            if (!await _campusFacade.IsMaintainerInDepartmentAsync(request.NewMaintainerId, departmentId, ct))
                throw new DomainException("MAINTAINER_NOT_IN_DEPT",
                    "The selected maintainer does not belong to your department.");

            ticket.ReassignMaintainer(request.NewMaintainerId, request.DeptManagerUserId);

            await _db.SaveChangesAsync(ct);
        }
    }
}
