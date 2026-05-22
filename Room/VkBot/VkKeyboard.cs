using System.Text.Json;
using UAMS.Room.Models.Enums;

namespace UAMS.Room.VkBot
{
    internal static class VkKeyboard
    {
        /// <summary>
        /// Returns a keyboard with action buttons for the given status.
        /// Always includes a Status button so the maintainer is never stranded.
        /// </summary>
        public static string ForStatus(TicketStatus status)
            => Serialize(BuildRows(status), oneTime: false);

        /// <summary>Remove keyboard (used while waiting for free-text input).</summary>
        public static string Empty() => Serialize([], oneTime: true);

        private static (string Label, string Cmd)[][] BuildRows(TicketStatus status) => status switch
        {
            TicketStatus.SentForFix => new[]
            {
                new[] { ("✅ Fixed",        "fixed"),
                        ("🔧 Needs Parts",  "needs_parts_start") },
                new[] { ("❌ Irreparable",  "irreparable_start") },
                new[] { ("📊 Status",       "status") },
            },
            TicketStatus.AwaitingParts => new[]
            {
                new[] { ("✅ Fixed",        "fixed"),
                        ("❌ Irreparable",  "irreparable_start") },
                new[] { ("📊 Status",       "status") },
            },
            TicketStatus.InspectionRequested => new[]
            {
                new[] { ("🔍 Repairable",   "inspection_repairable_start"),
                        ("🔍 Irreparable",  "inspection_irreparable_start") },
                new[] { ("📊 Status",       "status") },
            },
            TicketStatus.SentForReplacement => new[]
            {
                new[] { ("✅ Replaced",     "replaced") },
                new[] { ("📊 Status",       "status") },
            },
            // For any state where maintainer has no action (InspectionDone, Fixed,
            // Replaced, Irreparable, etc.) keep the Status button so they're not stranded.
            _ => new[]
            {
                new[] { ("📊 Status",  "status") },
            },
        };

        private static string Serialize((string Label, string Cmd)[][] rows, bool oneTime)
        {
            var buttons = rows.Select(row =>
                row.Select(btn => new
                {
                    action = new
                    {
                        type    = "callback",
                        label   = btn.Label,
                        payload = JsonSerializer.Serialize(new { cmd = btn.Cmd }),
                    }
                }).ToArray()
            ).ToArray();

            return JsonSerializer.Serialize(new { one_time = oneTime, buttons, inline = false });
        }

        public static string HumanStatus(TicketStatus s) => s switch
        {
            TicketStatus.SentForFix             => "Sent for Fix — waiting for your action",
            TicketStatus.AwaitingParts          => "Awaiting Parts — waiting for your action",
            TicketStatus.InspectionRequested    => "Inspection Requested — waiting for your action",
            TicketStatus.SentForReplacement     => "Sent for Replacement — waiting for your action",
            TicketStatus.InspectionDone         => "Inspection Done — awaiting asset manager decision",
            TicketStatus.Fixed                  => "Fixed — awaiting confirmation",
            TicketStatus.Replaced               => "Replaced — awaiting confirmation",
            TicketStatus.Irreparable            => "Irreparable — awaiting asset manager decision",
            TicketStatus.ConfirmedFixed         => "Confirmed Fixed ✅",
            TicketStatus.Closed                 => "Closed",
            TicketStatus.EscalatedExternally    => "Escalated Externally",
            _                                   => s.ToString(),
        };
    }
}
