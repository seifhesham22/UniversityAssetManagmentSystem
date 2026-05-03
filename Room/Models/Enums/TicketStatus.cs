using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Room.Models.Enums
{
    public enum TicketStatus
    {
        Open,
        InspectionRequested,
        InspectionDone,
        SentForFix,
        AwaitingParts,
        Fixed,
        SentForReplacement,
        Replaced,
        ConfirmedFixed,
        EscalatedExternally,
        Irreparable,
        Closed
    }
}