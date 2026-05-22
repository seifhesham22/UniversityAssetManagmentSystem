using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using Shared.Enums;
using UAMS.Room.Facades;
using UAMS.Room.Models.Enums;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.TicketFeatures.GetDeptActionCount
{
    public sealed record GetDeptActionCountQuery(Guid DeptManagerUserId) : IRequest<int>;

    internal sealed class GetDeptActionCountQueryHandler(
        RoomDesignDbContext _db,
        ICampusFacade _campusFacade)
        : IRequestHandler<GetDeptActionCountQuery, int>
    {
        private static readonly TicketStatus[] TerminalStatuses =
        [
            TicketStatus.Closed,
            TicketStatus.ConfirmedFixed,
            TicketStatus.EscalatedExternally,
        ];

        public async Task<int> Handle(GetDeptActionCountQuery request, CancellationToken ct)
        {
            var departmentId = await _campusFacade.GetDepartmentManagerDepartmentIdAsync(request.DeptManagerUserId, ct)
                ?? throw new DomainException("DEPT_MANAGER_NOT_FOUND", "Department manager profile not found.");

            return await _db.Tickets
                .AsNoTracking()
                .CountAsync(t =>
                    t.DepartmentId == departmentId &&
                    t.MaintainerId == null &&
                    !TerminalStatuses.Contains(t.Status), ct);
        }
    }
}
