using System.Text.Json;
using UAMS.Room.Models.Enums;

namespace UAMS.Room.VkBot
{
    internal static class VkKeyboard
    {
        public static string ForStatus(TicketStatus status)
            => Serialize(BuildRows(status), oneTime: false);

        public static string Empty() => Serialize([], oneTime: true);

        /// <summary>Keyboard letting the maintainer pick one of their active tickets.</summary>
        public static string TicketList(IEnumerable<(Guid Id, string AssetName, TicketStatus Status)> tickets)
        {
            var rows = tickets.Select(t =>
            {
                var label = Truncate($"{t.AssetName} [{ShortStatus(t.Status)}]", 40);
                return new[]
                {
                    new
                    {
                        action = new
                        {
                            type    = "callback",
                            label,
                            payload = JsonSerializer.Serialize(new { cmd = "use_ticket", id = t.Id.ToString() }),
                        }
                    }
                };
            }).ToArray();

            return JsonSerializer.Serialize(new { one_time = false, buttons = rows, inline = false });
        }

        private static (string Label, string Cmd)[][] BuildRows(TicketStatus status) => status switch
        {
            TicketStatus.SentForFix => new[]
            {
                new[] { ("✅ Fixed",        "fixed"),
                        ("🔧 Needs Parts",  "needs_parts_start") },
                new[] { ("❌ Irreparable",  "irreparable_start") },
                new[] { ("📊 Status",       "status"),
                        ("📋 All Tickets",  "list_tickets") },
            },
            TicketStatus.AwaitingParts => new[]
            {
                new[] { ("✅ Fixed",        "fixed"),
                        ("❌ Irreparable",  "irreparable_start") },
                new[] { ("📊 Status",       "status"),
                        ("📋 All Tickets",  "list_tickets") },
            },
            TicketStatus.InspectionRequested => new[]
            {
                new[] { ("🔍 Repairable",   "inspection_repairable_start"),
                        ("🔍 Irreparable",  "inspection_irreparable_start") },
                new[] { ("📊 Status",       "status"),
                        ("📋 All Tickets",  "list_tickets") },
            },
            TicketStatus.SentForReplacement => new[]
            {
                new[] { ("✅ Replaced",     "replaced") },
                new[] { ("📊 Status",       "status"),
                        ("📋 All Tickets",  "list_tickets") },
            },
            _ => new[]
            {
                new[] { ("📊 Status",       "status"),
                        ("📋 All Tickets",  "list_tickets") },
            },
        };

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

        private static string ShortStatus(TicketStatus s) => s switch
        {
            TicketStatus.SentForFix             => "Fix",
            TicketStatus.AwaitingParts          => "Parts",
            TicketStatus.InspectionRequested    => "Inspect",
            TicketStatus.SentForReplacement     => "Replace",
            TicketStatus.InspectionDone         => "Done",
            TicketStatus.Fixed                  => "Fixed",
            TicketStatus.Replaced               => "Replaced",
            TicketStatus.Irreparable            => "Irreparable",
            _                                   => s.ToString(),
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

        private static string Truncate(string s, int max)
            => s.Length <= max ? s : s[..max];
    }
}
