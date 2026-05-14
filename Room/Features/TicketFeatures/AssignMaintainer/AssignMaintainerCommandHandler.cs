using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.TicketFeatures.AssignMaintainer
{
    public sealed record AssignMaintainerCommand(
        Guid TicketId,
        Guid DeptManagerUserId,
        Guid MaintainerId) : IRequest;

    internal sealed class AssignMaintainerCommandHandler(
        RoomDesignDbContext _db,
        ICampusFacade _campusFacade)
        : IRequestHandler<AssignMaintainerCommand>
    {
        public async Task Handle(AssignMaintainerCommand request, CancellationToken ct)
        {
            var departmentId = await _campusFacade.GetDepartmentManagerDepartmentIdAsync(request.DeptManagerUserId, ct)
                ?? throw new DomainException("DEPT_MANAGER_NOT_FOUND", "Department manager profile not found.");

            var ticket = await _db.Tickets.FirstOrDefaultAsync(t => t.Id == request.TicketId, ct)
                ?? throw new DomainException("TICKET_NOT_FOUND", "Ticket not found.");

            if (ticket.DepartmentId != departmentId)
                throw new DomainException("TICKET_WRONG_DEPARTMENT",
                    "This ticket is not routed to your department.");

            if (!await _campusFacade.IsMaintainerInDepartmentAsync(request.MaintainerId, departmentId, ct))
                throw new DomainException("MAINTAINER_NOT_IN_DEPT",
                    "The selected maintainer does not belong to your department.");

            ticket.AssignToMaintainer(request.MaintainerId);

            await _db.SaveChangesAsync(ct);
        }
    }
}
