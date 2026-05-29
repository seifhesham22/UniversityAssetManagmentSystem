using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Presistence;
using UAMS.Room.ViewDtos;

namespace UAMS.Room.Features.RoomManagment.GetRoomById
{
    public sealed record GetRoomByIdQueryCommand(Guid RoomId) : IRequest<RoomDetailView>;
    public sealed class GetRoomByIdQueryHandler(RoomDesignDbContext _db)
        : IRequestHandler<GetRoomByIdQueryCommand, RoomDetailView>
    {
        public async Task<RoomDetailView> Handle(GetRoomByIdQueryCommand request, CancellationToken cancellationToken)
        {
            var existingRoom = await _db.Rooms.
                Include(room => room.Layout)
                .FirstOrDefaultAsync(room => room.Id == request.RoomId, cancellationToken)
                ?? throw new InvalidOperationException($"couldn't find a room with Id {request.RoomId}");

            return new RoomDetailView(
                Id: existingRoom.Id,
                Name: existingRoom.Name,
                BuildingId: existingRoom.BuildingId,
                FacultyId: existingRoom.FacultyId,
                Status: existingRoom.Status.ToString(),
                ClosureReason: existingRoom.ClosureReason,
                DesignedByUserId: existingRoom.DesignedByUserId,
                Layout: new LayoutView(
                    existingRoom.Layout
                    .PlacedAssets.Select(placedAsset => new PlacedAssetView(
                        placedAsset.Id,
                        placedAsset.AssetDefinitionId,
                        placedAsset.AssetName,
                        placedAsset.X,
                        placedAsset.Y,
                        placedAsset.Width,
                        placedAsset.Height,
                        placedAsset.Rotation,
                        placedAsset.Condition.ToString(),
                        placedAsset.GroupId,
                        placedAsset.GroupLabel,
                        placedAsset.CanvasRoomId,
                        placedAsset.CompositeId)).ToList(),
                    existingRoom.Layout.LastModifiedDate,
                    existingRoom.Layout.LastModifiedUserId));
        }
    }
}