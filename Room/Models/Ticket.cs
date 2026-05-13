using Shared.Abstractions;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using UAMS.Room.Models.Enums;

namespace UAMS.Room.Models
{
    public sealed class Ticket
    {
        public Guid Id { get; private set; }
        public Guid PlacedAssetId { get; private set; }
        public Guid RoomId { get; private set; }
        public Guid FacultyId { get; private set; }
        public Guid ReporterId { get; private set; }
        public Guid? DepartmentId { get; private set; }
        public Guid? MaintainerId { get; private set; }
        public Guid? ConfirmedByUserId { get; private set; }
        public TicketStatus Status { get; private set; }
        public TicketDecision Decision { get; private set; }

        private readonly List<TicketNote> _ticketNotes = new();
        public IReadOnlyCollection<TicketNote> TicketNotes => _ticketNotes;
        public DateTime CreatedAt { get; private set; }
        public DateTime LastUpdatedAt { get; private set; }
        private static readonly HashSet<(TicketStatus From, TicketStatus To)> AllowedTransitions = new()
    {
        (TicketStatus.Open, TicketStatus.EscalatedExternally),
        (TicketStatus.Open, TicketStatus.InspectionRequested),
        (TicketStatus.Open, TicketStatus.SentForFix),
        (TicketStatus.Open, TicketStatus.SentForReplacement),

        (TicketStatus.InspectionRequested, TicketStatus.InspectionDone),
        (TicketStatus.InspectionRequested, TicketStatus.Irreparable),

        (TicketStatus.InspectionDone, TicketStatus.SentForFix),
        (TicketStatus.InspectionDone, TicketStatus.SentForReplacement),

        (TicketStatus.SentForFix, TicketStatus.Fixed),
        (TicketStatus.SentForFix, TicketStatus.AwaitingParts),
        (TicketStatus.SentForFix, TicketStatus.Irreparable),
        (TicketStatus.AwaitingParts, TicketStatus.Fixed),
        (TicketStatus.AwaitingParts, TicketStatus.Irreparable),

        (TicketStatus.SentForReplacement, TicketStatus.Replaced),
        (TicketStatus.Irreparable, TicketStatus.SentForReplacement),

        (TicketStatus.Fixed, TicketStatus.ConfirmedFixed),
        (TicketStatus.Replaced, TicketStatus.ConfirmedFixed),

        (TicketStatus.ConfirmedFixed, TicketStatus.Closed),
        (TicketStatus.EscalatedExternally, TicketStatus.Closed),
        (TicketStatus.Irreparable, TicketStatus.Closed),
    };

        private Ticket() { }

        public Ticket(Guid placedAssetId, Guid roomId, Guid facultyId, Guid reporterId)
        {
            if (placedAssetId == Guid.Empty) throw new ArgumentException(nameof(placedAssetId));
            if (roomId == Guid.Empty) throw new ArgumentException(nameof(roomId));
            if (facultyId == Guid.Empty) throw new ArgumentException(nameof(facultyId));
            if (reporterId == Guid.Empty) throw new ArgumentException(nameof(reporterId));

            Id = Guid.NewGuid();
            PlacedAssetId = placedAssetId;
            RoomId = roomId;
            FacultyId = facultyId;
            ReporterId = reporterId;
            this.Status = TicketStatus.Open;
            this.Decision = TicketDecision.Pending;
            CreatedAt = DateTime.UtcNow;
            LastUpdatedAt = DateTime.UtcNow;
        }

        public void AssignToMaintainer(Guid maintainerId)
        {
            if (MaintainerId.HasValue)
                throw new InvalidOperationException(
                    "already assigned, unassign the maintainer from ticket and then assign the new one");

            MaintainerId = maintainerId;
            LastUpdatedAt = DateTime.UtcNow;
        }


        //Asset Manager
        public void EsculateExternally(Guid userId, string? note = null)
            => TransitionTo(
                userId,
                note ?? "Ticket escalated to external department",
                TicketStatus.EscalatedExternally);

        public void SendForInspection(Guid userId, Guid departmentId, string? note = null)
            => TransitionTo(
                userId,
                note ?? "Ticket sent for inspection",
                TicketStatus.InspectionRequested,
                TicketDecision.SendForInspection,
                departmentId);

        public void SendForFix(Guid userId, Guid departmentId, string? note = null)
            => TransitionTo(
                userId,
                note ?? "Ticket sent for fix",
                TicketStatus.SentForFix,
                TicketDecision.SendForFix,
                departmentId);

        public void SendForReplacement(Guid userId, Guid departmentId, string? note = null)
            => TransitionTo(
                userId,
                note ?? "Ticket sent for replacement request",
                TicketStatus.SentForReplacement,
                TicketDecision.SendForReplacement,
                departmentId);

        //Maintainer
        public void MarkFixed(Guid maintainerId)
        {
            if (MaintainerId != maintainerId)
                throw new DomainException("TICKET_NOT_ASSIGNED_TO_YOU",
                    "This ticket is not assigned to you.");

            TransitionTo(maintainerId, "Asset fixed.", TicketStatus.Fixed);
        }

        public void MarkReplaced(Guid maintainerId)
        {
            if (MaintainerId != maintainerId)
                throw new DomainException("TICKET_NOT_ASSIGNED_TO_YOU",
                    "This ticket is not assigned to you.");

            TransitionTo(maintainerId, "Asset replaced.", TicketStatus.Replaced);
        }

        public void ReportNeedsParts(Guid maintainerId, string reason)
        {
            if (MaintainerId != maintainerId)
                throw new DomainException("TICKET_NOT_ASSIGNED_TO_YOU",
                    "This ticket is not assigned to you.");

            RequireReason(reason);
            TransitionTo(maintainerId, $"Needs parts: {reason}", TicketStatus.AwaitingParts);
        }

        public void ReportIrreparable(Guid maintainerId, string reason)
        {
            if (MaintainerId != maintainerId)
                throw new DomainException("TICKET_NOT_ASSIGNED_TO_YOU",
                    "This ticket is not assigned to you.");

            RequireReason(reason);
            TransitionTo(maintainerId, $"Irreparable: {reason}", TicketStatus.Irreparable);
        }

        public void SubmitInspectionResult(Guid maintainerId, bool isRepairable, string notes)
        {
            if (MaintainerId != maintainerId)
                throw new DomainException("TICKET_NOT_ASSIGNED_TO_YOU",
                    "This ticket is not assigned to you.");

            if (string.IsNullOrWhiteSpace(notes))
                throw new DomainException("INSPECTION_NOTES_REQUIRED", "Notes required.");

            TransitionTo(
                maintainerId,
                notes,
                isRepairable 
                ? TicketStatus.InspectionDone
                : TicketStatus.Irreparable);
        }

        public void ConfirmFixed(Guid userId)
        {
            TransitionTo(userId, "Fix confirmed.", TicketStatus.ConfirmedFixed);
            ConfirmedByUserId = userId;
        }

        public void Close(Guid userId, string? note = null)
            => TransitionTo(userId, note ?? "Ticket closed.", TicketStatus.Closed);

        public TicketNote AddNote(Guid authorId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new DomainException("TICKET_NOTE_EMPTY", "Content required.");
            var note = new TicketNote(Id, authorId, content.Trim());
            _ticketNotes.Add(note);
            LastUpdatedAt = DateTime.UtcNow;
            return note;
        }

        public PlacedAssetCondition? GetConditionForCurrentStatus() => Status switch
        {
            TicketStatus.Open => PlacedAssetCondition.Reported,
            TicketStatus.InspectionRequested => PlacedAssetCondition.Reported,
            TicketStatus.InspectionDone => PlacedAssetCondition.Reported,
            TicketStatus.SentForFix => PlacedAssetCondition.UnderMaintenance,
            TicketStatus.AwaitingParts => PlacedAssetCondition.UnderMaintenance,
            TicketStatus.Fixed => PlacedAssetCondition.Good,
            TicketStatus.SentForReplacement => PlacedAssetCondition.UnderMaintenance,
            TicketStatus.Replaced => PlacedAssetCondition.Replaced,
            TicketStatus.ConfirmedFixed => PlacedAssetCondition.Good,
            TicketStatus.Irreparable => PlacedAssetCondition.Irreparable,
            TicketStatus.EscalatedExternally => null,
            TicketStatus.Closed => null,
            _ => null
        };

        private void TransitionTo(
            Guid actorId,
            string note,
            TicketStatus target,
            TicketDecision? decision = null,
            Guid? departmentId = null)
        {
            if(!AllowedTransitions.Contains((this.Status, target)))
            {
                throw new DomainException("TICKET_INVALID_TRANSITION", $"Cannot move from {Status} to {target}.");
            }

            if(departmentId.HasValue && departmentId.Value == Guid.Empty)
            {
                throw new DomainException("TICKET_DEPT_INVALID", "Department required.");
            }

            if(departmentId.HasValue)
                this.DepartmentId = departmentId.Value;

            if (decision.HasValue)
                this.Decision = decision.Value;

            this.Status = target;
            _ticketNotes.Add(new TicketNote(Id, actorId, note));
            this.LastUpdatedAt = DateTime.UtcNow;
        }
        private static void RequireReason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("TICKET_REASON_REQUIRED", "Reason required.");
        }
    }
}