using MediatR;
using Microsoft.EntityFrameworkCore;
using UAMS.Room.Models.Enums;
using UAMS.Room.Presistence;

namespace UAMS.Room.Features.TicketFeatures.GetAmActionCount
{
    public sealed record GetAmActionCountQuery(Guid FacultyId) : IRequest<int>;

    internal sealed class GetAmActionCountQueryHandler(RoomDesignDbContext _db)
        : IRequestHandler<GetAmActionCountQuery, int>
    {
        private static readonly TicketStatus[] ActionStatuses =
        [
            TicketStatus.InspectionDone,
            TicketStatus.Irreparable,
            TicketStatus.Fixed,
            TicketStatus.Replaced,
        ];

        public async Task<int> Handle(GetAmActionCountQuery request, CancellationToken ct)
            => await _db.Tickets
                .AsNoTracking()
                .CountAsync(t => t.FacultyId == request.FacultyId && ActionStatuses.Contains(t.Status), ct);
    }
}
