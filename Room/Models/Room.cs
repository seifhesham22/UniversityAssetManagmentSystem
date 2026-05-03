using Shared.Abstractions;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Room.Models.Enums;

namespace UAMS.Room.Models
{
    public sealed class Room
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public Guid BuildingId { get; private set; }
        public Guid FacultyId { get; private set; }
        public RoomStatus Status { get; private set; }
        public string? ClosureReason { get; private set; }
        public Guid DesignedByUserId { get; private set; }
        public RoomLayout Layout { get; private set; } = new();

        private Room() { }

        public Room(string name, Guid buildingId, Guid facultyId, Guid designedByUserId)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(nameof(name));
            if (buildingId == Guid.Empty) throw new ArgumentException(nameof(buildingId));
            if (facultyId == Guid.Empty) throw new ArgumentException(nameof(facultyId));
            if (designedByUserId == Guid.Empty) throw new ArgumentException(nameof(designedByUserId));

            Id = Guid.NewGuid();
            Name = name.Trim();
            BuildingId = buildingId;
            FacultyId = facultyId;
            Status = RoomStatus.Open;
            DesignedByUserId = designedByUserId;
        }

        public void Close(string reason)
        {
            if (Status == RoomStatus.Closed)
                throw new DomainException("ROOM_ALREADY_CLOSED", "Room is already closed.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("ROOM_CLOSURE_REASON_REQUIRED", "Closure reason required.");
            Status = RoomStatus.Closed;
            ClosureReason = reason.Trim();
        }

        public void Reopen()
        {
            if (Status == RoomStatus.Open)
                throw new DomainException("ROOM_ALREADY_OPEN", "Room is already open.");
            Status = RoomStatus.Open;
            ClosureReason = null;
        }
    }
}