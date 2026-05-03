using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Ticket.Enums
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
    public enum TicketDecision
    {
        Pending,
        EscalateExternally,
        SendForInspection,
        SendForFix,
        SendForReplacement
    }
}