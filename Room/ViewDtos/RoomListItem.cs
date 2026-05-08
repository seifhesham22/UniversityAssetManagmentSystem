using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Room.ViewDtos
{
    public sealed record RoomListItem(
    Guid Id,
    string Name,
    Guid BuildingId,
    Guid FacultyId,
    string Status,
    int AssetCount);
}