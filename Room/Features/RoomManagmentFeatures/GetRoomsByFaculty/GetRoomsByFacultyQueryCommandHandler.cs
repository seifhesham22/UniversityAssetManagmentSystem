using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Presistence;
using UAMS.Room.ViewDtos;

namespace UAMS.Room.Features.RoomManagment.GetRoomsByFaculty
{
    public sealed record GetRoomsByFacultyQueryCommand(
        Guid FacultyId,
        int page = 1,
        int totalSize = 20)
        : IRequest<PagedResult<RoomListItem>>;

    public sealed class GetRoomsByFacultyQueryCommandHandler(RoomDesignDbContext _db)
        : IRequestHandler<GetRoomsByFacultyQueryCommand, PagedResult<RoomListItem>>
    {
        public async Task<PagedResult<RoomListItem>> Handle(
            GetRoomsByFacultyQueryCommand request,
            CancellationToken cancellationToken)
        {
            var allRooms = _db.Rooms
                .AsNoTracking()
                .Where(room => room.FacultyId == request.FacultyId);

            var total = await allRooms.CountAsync();
            var Items = await allRooms.OrderBy(x => x.Name)
                .Skip((request.page - 1) * request.totalSize)
                .Take(total)
                .Select(x => new RoomListItem(
                    Id: x.Id,
                    Name: x.Name,
                    BuildingId: x.BuildingId,
                    FacultyId: x.FacultyId,
                    Status: x.Status.ToString(),
                    AssetCount: x.Layout.PlacedAssets.Count))
                .ToListAsync(cancellationToken);

            return new PagedResult<RoomListItem>(Items, total, request.page, request.totalSize);
        }
    }
}