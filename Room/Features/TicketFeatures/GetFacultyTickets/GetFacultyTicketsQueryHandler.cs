using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;
using UAMS.Room.ViewDtos;

namespace UAMS.Room.Features.TicketFeatures.GetFacultyTickets
{
    public sealed record GetFacultyTicketsQuery(
        Guid AssetManagerUserId,
        Guid FacultyId) : IRequest<List<TicketListItem>>;

    internal sealed class GetFacultyTicketsQueryHandler(
        RoomDesignDbContext _db,
        IFacultyFacade _facultyFacade,
        ICampusFacade _campusFacade)
        : IRequestHandler<GetFacultyTicketsQuery, List<TicketListItem>>
    {
        public async Task<List<TicketListItem>> Handle(GetFacultyTicketsQuery request, CancellationToken ct)
        {
            var isManager = await _facultyFacade.IsAssetManagerOfFaculty(request.AssetManagerUserId, request.FacultyId);
            if (!isManager)
                throw new DomainException("UNAUTHORIZED", "You are not the asset manager of this faculty.");

            var tickets = await _db.Tickets
                .AsNoTracking()
                .Where(t => t.FacultyId == request.FacultyId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync(ct);

            if (tickets.Count == 0)
                return [];

            var roomIds = tickets.Select(t => t.RoomId).Distinct().ToList();
            var rooms = await _db.Rooms
                .AsNoTracking()
                .Include(r => r.Layout)
                .Where(r => roomIds.Contains(r.Id))
                .ToListAsync(ct);
            var roomLookup = rooms.ToDictionary(r => r.Id);

            var reporterIds = tickets.Select(t => t.ReporterId).Distinct().ToList();
            var reporterNames = await _campusFacade.GetUserNamesAsync(reporterIds, ct);

            var maintainerIds = tickets
                .Where(t => t.MaintainerId.HasValue)
                .Select(t => t.MaintainerId!.Value)
                .Distinct()
                .ToList();

            var maintainerNames = new Dictionary<Guid, string?>();
            foreach (var mid in maintainerIds)
            {
                maintainerNames[mid] = await _campusFacade.GetMaintainerNameAsync(mid, ct);
            }

            var deptIds = tickets
                .Where(t => t.DepartmentId.HasValue)
                .Select(t => t.DepartmentId!.Value)
                .Distinct()
                .ToList();

            var deptNames = new Dictionary<Guid, string?>();
            foreach (var did in deptIds)
            {
                deptNames[did] = await _campusFacade.GetDepartmentNameAsync(did, ct);
            }

            return tickets.Select(t =>
            {
                roomLookup.TryGetValue(t.RoomId, out var room);
                var assetName = room?.Layout.FindAsset(t.PlacedAssetId)?.AssetName ?? "Unknown";
                var roomName = room?.Name ?? "Unknown";

                return new TicketListItem(
                    Id: t.Id,
                    AssetName: assetName,
                    RoomName: roomName,
                    RoomCode: roomName,
                    FacultyId: t.FacultyId,
                    ReportedByName: reporterNames.GetValueOrDefault(t.ReporterId, "Unknown"),
                    Status: t.Status.ToString(),
                    Decision: t.Decision.ToString(),
                    AssignedToDepartmentId: t.DepartmentId,
                    DepartmentName: t.DepartmentId.HasValue
                        ? deptNames.GetValueOrDefault(t.DepartmentId.Value)
                        : null,
                    CurrentMaintainerId: t.MaintainerId,
                    CurrentMaintainerName: t.MaintainerId.HasValue
                        ? maintainerNames.GetValueOrDefault(t.MaintainerId.Value)
                        : null,
                    CreatedAtUtc: t.CreatedAt,
                    UpdatedAtUtc: t.LastUpdatedAt);
            }).ToList();
        }
    }
}
