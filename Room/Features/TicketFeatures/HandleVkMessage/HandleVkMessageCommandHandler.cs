using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Shared.Abstractions;
using System.Text;
using System.Text.Json;
using UAMS.Room.Facades;
using UAMS.Room.Models;
using UAMS.Room.Models.Enums;
using UAMS.Room.Presistence;
using UAMS.Room.VkBot;

namespace UAMS.Room.Features.TicketFeatures.HandleVkMessage
{
    public sealed record HandleVkMessageCommand(
        string VkUserId,
        string Text,
        string? Payload = null) : IRequest;

    internal sealed class HandleVkMessageCommandHandler(
        RoomDesignDbContext _db,
        ICampusFacade _campusFacade,
        IVkBotService _vkBot,
        IMemoryCache _cache)
        : IRequestHandler<HandleVkMessageCommand>
    {
        private static readonly TicketStatus[] TerminalStatuses =
        [
            TicketStatus.Closed, TicketStatus.ConfirmedFixed, TicketStatus.EscalatedExternally
        ];

        private string PendingCacheKey(string vkId)        => $"vk_pending_{vkId}";
        private string SelectedTicketCacheKey(string vkId) => $"vk_ticket_{vkId}";

        public async Task Handle(HandleVkMessageCommand request, CancellationToken ct)
        {
            var maintainerId = await _campusFacade.GetMaintainerIdByVkIdAsync(request.VkUserId, ct);
            if (maintainerId == null)
            {
                await _vkBot.SendMessageAsync(request.VkUserId,
                    "❌ Your VK ID is not registered. Contact your department manager.", ct: ct);
                return;
            }

            // Load ALL active tickets for this maintainer (ordered most-recent first)
            var allTickets = await _db.Tickets
                .Include(t => t.TicketNotes)
                .Where(t =>
                    t.MaintainerId == maintainerId &&
                    !TerminalStatuses.Contains(t.Status))
                .OrderByDescending(t => t.LastUpdatedAt)
                .ToListAsync(ct);

            if (allTickets.Count == 0)
            {
                await _vkBot.SendMessageAsync(request.VkUserId,
                    "📋 No active tickets assigned to you.", ct: ct);
                _cache.Remove(SelectedTicketCacheKey(request.VkUserId));
                return;
            }

            // Resolve current working ticket (from cache selection or default to most recent)
            Ticket ticket = allTickets[0];
            if (_cache.TryGetValue<Guid>(SelectedTicketCacheKey(request.VkUserId), out var selectedId))
            {
                var cached = allTickets.FirstOrDefault(t => t.Id == selectedId);
                if (cached != null) ticket = cached;
                // If cached ticket is no longer active, fall back to most recent and clear
                else _cache.Remove(SelectedTicketCacheKey(request.VkUserId));
            }

            var (cmd, payloadTicketId) = ParsePayload(request.Payload);
            cmd ??= request.Text.Trim().ToLowerInvariant();

            // ── Pending state: waiting for free-text input ───────────────────
            if (_cache.TryGetValue<string>(PendingCacheKey(request.VkUserId), out var pendingCmd))
            {
                if (!HasButtonPayload(request.Payload))
                {
                    _cache.Remove(PendingCacheKey(request.VkUserId));
                    await ExecutePendingCommand(request, ticket, maintainerId.Value, pendingCmd!, ct);
                    return;
                }
                _cache.Remove(PendingCacheKey(request.VkUserId));
            }

            // ── Dispatch ─────────────────────────────────────────────────────
            switch (cmd)
            {
                case "list_tickets":
                    await SendTicketList(request.VkUserId, allTickets, ct);
                    break;

                case "use_ticket":
                    if (payloadTicketId.HasValue)
                    {
                        var chosen = allTickets.FirstOrDefault(t => t.Id == payloadTicketId.Value);
                        if (chosen != null)
                        {
                            _cache.Set(SelectedTicketCacheKey(request.VkUserId),
                                chosen.Id, TimeSpan.FromMinutes(30));
                            await SendStatus(request.VkUserId, chosen, ct);
                            return;
                        }
                    }
                    await SendTicketList(request.VkUserId, allTickets, ct);
                    break;

                case "fixed":
                    await ExecuteTransition(request.VkUserId, ticket, maintainerId.Value,
                        t => t.MarkFixed(maintainerId.Value),
                        "✅ Marked as fixed! The asset manager will confirm.", ct);
                    break;

                case "replaced":
                    await ExecuteTransition(request.VkUserId, ticket, maintainerId.Value,
                        t => t.MarkReplaced(maintainerId.Value),
                        "✅ Marked as replaced! The asset manager will confirm.", ct);
                    break;

                case "needs_parts_start":
                    _cache.Set(PendingCacheKey(request.VkUserId), "needs_parts", TimeSpan.FromMinutes(5));
                    await _vkBot.SendMessageAsync(request.VkUserId,
                        "🔧 Needs Parts\n\nType what parts are required and send:\n(just type — no prefix needed)",
                        VkKeyboard.Empty(), ct);
                    break;

                case "irreparable_start":
                    _cache.Set(PendingCacheKey(request.VkUserId), "irreparable", TimeSpan.FromMinutes(5));
                    await _vkBot.SendMessageAsync(request.VkUserId,
                        "❌ Irreparable\n\nType the reason and send:\n(just type — no prefix needed)",
                        VkKeyboard.Empty(), ct);
                    break;

                case "inspection_repairable_start":
                    _cache.Set(PendingCacheKey(request.VkUserId), "inspection_repairable", TimeSpan.FromMinutes(5));
                    await _vkBot.SendMessageAsync(request.VkUserId,
                        "🔍 Inspection — Repairable\n\nType your inspection notes and send:\n(just type — no prefix needed)",
                        VkKeyboard.Empty(), ct);
                    break;

                case "inspection_irreparable_start":
                    _cache.Set(PendingCacheKey(request.VkUserId), "inspection_irreparable", TimeSpan.FromMinutes(5));
                    await _vkBot.SendMessageAsync(request.VkUserId,
                        "🔍 Inspection — Irreparable\n\nType your inspection notes and send:\n(just type — no prefix needed)",
                        VkKeyboard.Empty(), ct);
                    break;

                default: // "status" or anything unknown → show current ticket status
                    await SendStatus(request.VkUserId, ticket, ct);
                    break;
            }
        }

        // ── Ticket list ────────────────────────────────────────────────────────

        private async Task SendTicketList(
            string vkUserId,
            List<Ticket> tickets,
            CancellationToken ct)
        {
            if (tickets.Count == 1)
            {
                // Auto-select the only active ticket
                await SendStatus(vkUserId, tickets[0], ct);
                return;
            }

            var roomIds   = tickets.Select(t => t.RoomId).Distinct().ToList();
            var rooms     = await _db.Rooms.AsNoTracking().Include(r => r.Layout)
                .Where(r => roomIds.Contains(r.Id)).ToListAsync(ct);
            var roomLookup = rooms.ToDictionary(r => r.Id);

            var sb = new StringBuilder($"📋 You have {tickets.Count} active tickets. Choose one:\n\n");

            var ticketEntries = tickets.Select(t =>
            {
                roomLookup.TryGetValue(t.RoomId, out var room);
                var assetName = room?.Layout.FindAsset(t.PlacedAssetId)?.AssetName ?? "Unknown";
                sb.AppendLine($"• {assetName} — {room?.Name ?? "?"} [{VkKeyboard.HumanStatus(t.Status)}]");
                return (t.Id, assetName, t.Status);
            }).ToList();

            var keyboard = VkKeyboard.TicketList(ticketEntries);
            await _vkBot.SendMessageAsync(vkUserId, sb.ToString(), keyboard, ct);
        }

        // ── Status ────────────────────────────────────────────────────────────

        private async Task SendStatus(string vkUserId, Ticket ticket, CancellationToken ct)
        {
            var room = await _db.Rooms.AsNoTracking().Include(r => r.Layout)
                .FirstOrDefaultAsync(r => r.Id == ticket.RoomId, ct);

            var assetName  = room?.Layout.FindAsset(ticket.PlacedAssetId)?.AssetName ?? "Unknown";
            var statusText = VkKeyboard.HumanStatus(ticket.Status);
            var keyboard   = VkKeyboard.ForStatus(ticket.Status);

            var noteSection = new System.Text.StringBuilder();
            if (ticket.TicketNotes.Count > 0)
            {
                var authorIds   = ticket.TicketNotes.Select(n => n.AuthorId).Distinct().ToList();
                var authorNames = await _campusFacade.GetNoteAuthorNamesAsync(authorIds, ct);
                noteSection.AppendLine();
                noteSection.AppendLine("📝 Notes:");
                foreach (var n in ticket.TicketNotes.OrderBy(n => n.CreatedAtUtc).TakeLast(5))
                    noteSection.AppendLine($"  [{n.CreatedAtUtc:dd.MM HH:mm}] {authorNames.GetValueOrDefault(n.AuthorId, "Unknown")}: {n.Content}");
            }

            await _vkBot.SendMessageAsync(vkUserId,
                $"📋 Ticket Status\n" +
                $"Asset: {assetName}\n" +
                $"Room:  {room?.Name ?? "Unknown"}\n" +
                $"Status: {statusText}" +
                noteSection.ToString(),
                keyboard, ct);
        }

        // ── Pending free-text input ────────────────────────────────────────────

        private async Task ExecutePendingCommand(
            HandleVkMessageCommand request,
            Ticket ticket,
            Guid maintainerId,
            string pendingCmd,
            CancellationToken ct)
        {
            var input = request.Text.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                await _vkBot.SendMessageAsync(request.VkUserId,
                    "⚠️ Input was empty. Please try again.",
                    VkKeyboard.ForStatus(ticket.Status), ct);
                return;
            }

            try
            {
                (Action<Ticket> action, string successMsg) = pendingCmd switch
                {
                    "needs_parts"             => ((Action<Ticket>)(t => t.ReportNeedsParts(maintainerId, input)),   "🔧 Reported: needs parts."),
                    "irreparable"             => ((Action<Ticket>)(t => t.ReportIrreparable(maintainerId, input)),  "❌ Reported as irreparable."),
                    "inspection_repairable"   => ((Action<Ticket>)(t => t.SubmitInspectionResult(maintainerId, true,  input)), "🔍 Inspection submitted: repairable."),
                    "inspection_irreparable"  => ((Action<Ticket>)(t => t.SubmitInspectionResult(maintainerId, false, input)), "🔍 Inspection submitted: irreparable."),
                    _                         => throw new InvalidOperationException("Unknown pending command."),
                };

                await ExecuteTransition(request.VkUserId, ticket, maintainerId, action, successMsg, ct);
            }
            catch (DomainException ex)
            {
                await _vkBot.SendMessageAsync(request.VkUserId,
                    $"❌ {ex.Message}", VkKeyboard.ForStatus(ticket.Status), ct);
            }
        }

        // ── Domain transition + room layout sync ──────────────────────────────

        private async Task ExecuteTransition(
            string vkUserId,
            Ticket ticket,
            Guid maintainerId,
            Action<Ticket> action,
            string successMessage,
            CancellationToken ct)
        {
            try
            {
                action(ticket);

                var room = await _db.Rooms.Include(r => r.Layout)
                    .FirstOrDefaultAsync(r => r.Id == ticket.RoomId, ct);

                if (room != null)
                {
                    var condition = ticket.GetConditionForCurrentStatus();
                    if (condition != null)
                        room.Layout.UpdateAssetCondition(ticket.PlacedAssetId, condition.Value);
                }

                await _db.SaveChangesAsync(ct);

                await _vkBot.SendMessageAsync(vkUserId,
                    $"{successMessage}\n\nCurrent status: {VkKeyboard.HumanStatus(ticket.Status)}",
                    VkKeyboard.ForStatus(ticket.Status), ct);
            }
            catch (DomainException ex)
            {
                await _vkBot.SendMessageAsync(vkUserId,
                    $"❌ {ex.Message}", VkKeyboard.ForStatus(ticket.Status), ct);
            }
        }

        // ── Payload parsing ───────────────────────────────────────────────────

        private static (string? Cmd, Guid? TicketId) ParsePayload(string? payload)
        {
            if (string.IsNullOrEmpty(payload)) return (null, null);
            try
            {
                var root = JsonDocument.Parse(payload).RootElement;
                var cmd  = root.TryGetProperty("cmd", out var c) ? c.GetString() : null;
                Guid? id = null;
                if (root.TryGetProperty("id", out var idProp) &&
                    Guid.TryParse(idProp.GetString(), out var parsed))
                    id = parsed;
                return (cmd, id);
            }
            catch { return (null, null); }
        }

        private static bool HasButtonPayload(string? payload)
            => ParsePayload(payload).Cmd != null;
    }
}
