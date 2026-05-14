using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using UAMS.Room.Facades;
using UAMS.Room.Presistence;
using UAMS.Room.ViewDtos;

namespace UAMS.Room.Features.TicketFeatures.GetDepartmentTickets
{
    public sealed record GetDepartmentTicketsQuery(
        Guid DeptManagerUserId) : IRequest<List<DeptManagerTicketItem>>;

    internal sealed class GetDepartmentTicketsQueryHandler(
        RoomDesignDbContext _db,
        ICampusFacade _campusFacade)
        : IRequestHandler<GetDepartmentTicketsQuery, List<DeptManagerTicketItem>>
    {
        public async Task<List<DeptManagerTicketItem>> Handle(GetDepartmentTicketsQuery request, CancellationToken ct)
        {
            var departmentId = await _campusFacade.GetDepartmentManagerDepartmentIdAsync(request.DeptManagerUserId, ct)
                ?? throw new DomainException("DEPT_MANAGER_NOT_FOUND", "Department manager profile not found.");

            var tickets = await _db.Tickets
                .AsNoTracking()
                .Where(t => t.DepartmentId == departmentId)
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

            var facultyIds = rooms.Select(r => r.FacultyId).Distinct().ToList();
            var buildingIds = rooms.Select(r => r.BuildingId).Distinct().ToList();

            var facultyNames = new Dictionary<Guid, string?>();
            foreach (var fid in facultyIds)
                facultyNames[fid] = await _campusFacade.GetFacultyNameAsync(fid, ct);

            var buildingNames = new Dictionary<Guid, string?>();
            foreach (var bid in buildingIds)
                buildingNames[bid] = await _campusFacade.GetBuildingNameAsync(bid, ct);

            var maintainerIds = tickets
                .Where(t => t.MaintainerId.HasValue)
                .Select(t => t.MaintainerId!.Value)
                .Distinct()
                .ToList();

            var maintainerNames = new Dictionary<Guid, string?>();
            foreach (var mid in maintainerIds)
                maintainerNames[mid] = await _campusFacade.GetMaintainerNameAsync(mid, ct);

            return tickets.Select(t =>
            {
                roomLookup.TryGetValue(t.RoomId, out var room);
                var assetName = room?.Layout.FindAsset(t.PlacedAssetId)?.AssetName ?? "Unknown";
                var roomName = room?.Name ?? "Unknown";
                var facultyId = room?.FacultyId ?? t.FacultyId;
                var buildingId = room?.BuildingId ?? Guid.Empty;

                return new DeptManagerTicketItem(
                    Id: t.Id,
                    AssetName: assetName,
                    RoomId: t.RoomId,
                    RoomName: roomName,
                    FacultyId: facultyId,
                    FacultyName: facultyNames.GetValueOrDefault(facultyId) ?? "Unknown",
                    BuildingName: buildingNames.GetValueOrDefault(buildingId) ?? "Unknown",
                    Status: t.Status.ToString(),
                    Decision: t.Decision.ToString(),
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
