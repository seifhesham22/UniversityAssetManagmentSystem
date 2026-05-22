using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Shared.Abstractions;
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

        private string StateCacheKey(string vkId) => $"vk_pending_{vkId}";

        public async Task Handle(HandleVkMessageCommand request, CancellationToken ct)
        {
            var maintainerId = await _campusFacade.GetMaintainerIdByVkIdAsync(request.VkUserId, ct);
            if (maintainerId == null)
            {
                await _vkBot.SendMessageAsync(request.VkUserId,
                    "❌ Your VK ID is not registered. Contact your department manager.", ct: ct);
                return;
            }

            // Always pick the most-recently-updated active ticket to avoid stale state.
            var ticket = await _db.Tickets
                .Include(t => t.TicketNotes)
                .Where(t =>
                    t.MaintainerId == maintainerId &&
                    !TerminalStatuses.Contains(t.Status))
                .OrderByDescending(t => t.LastUpdatedAt)
                .FirstOrDefaultAsync(ct);

            if (ticket == null)
            {
                await _vkBot.SendMessageAsync(request.VkUserId,
                    "📋 No active tickets assigned to you.", ct: ct);
                return;
            }

            var cmd      = ParseCommand(request.Payload) ?? request.Text.Trim().ToLowerInvariant();
            var keyboard = VkKeyboard.ForStatus(ticket.Status);

            // ── Check pending state (waiting for free-text input) ────────────
            if (_cache.TryGetValue<string>(StateCacheKey(request.VkUserId), out var pendingCmd))
            {
                if (!HasButtonPayload(request.Payload))
                {
                    _cache.Remove(StateCacheKey(request.VkUserId));
                    await ExecutePendingCommand(request, ticket, maintainerId.Value, pendingCmd!, ct);
                    return;
                }
                // User pressed a new button before finishing — cancel pending state
                _cache.Remove(StateCacheKey(request.VkUserId));
            }

            // ── Dispatch ─────────────────────────────────────────────────────
            switch (cmd)
            {
                case "fixed":
                    await ExecuteTransition(
                        request.VkUserId, ticket, maintainerId.Value,
                        t => t.MarkFixed(maintainerId.Value),
                        "✅ Marked as fixed! The asset manager will confirm.", ct);
                    break;

                case "replaced":
                    await ExecuteTransition(
                        request.VkUserId, ticket, maintainerId.Value,
                        t => t.MarkReplaced(maintainerId.Value),
                        "✅ Marked as replaced! The asset manager will confirm.", ct);
                    break;

                case "needs_parts_start":
                    _cache.Set(StateCacheKey(request.VkUserId), "needs_parts", TimeSpan.FromMinutes(5));
                    await _vkBot.SendMessageAsync(request.VkUserId,
                        "🔧 Needs Parts\n\nType what parts are required and send:\n(just type — no prefix needed)",
                        VkKeyboard.Empty(), ct);
                    break;

                case "irreparable_start":
                    _cache.Set(StateCacheKey(request.VkUserId), "irreparable", TimeSpan.FromMinutes(5));
                    await _vkBot.SendMessageAsync(request.VkUserId,
                        "❌ Irreparable\n\nType the reason and send:\n(just type — no prefix needed)",
                        VkKeyboard.Empty(), ct);
                    break;

                case "inspection_repairable_start":
                    _cache.Set(StateCacheKey(request.VkUserId), "inspection_repairable", TimeSpan.FromMinutes(5));
                    await _vkBot.SendMessageAsync(request.VkUserId,
                        "🔍 Inspection — Repairable\n\nType your inspection notes and send:\n(just type — no prefix needed)",
                        VkKeyboard.Empty(), ct);
                    break;

                case "inspection_irreparable_start":
                    _cache.Set(StateCacheKey(request.VkUserId), "inspection_irreparable", TimeSpan.FromMinutes(5));
                    await _vkBot.SendMessageAsync(request.VkUserId,
                        "🔍 Inspection — Irreparable\n\nType your inspection notes and send:\n(just type — no prefix needed)",
                        VkKeyboard.Empty(), ct);
                    break;

                case "status":
                    await SendStatus(request.VkUserId, ticket, ct);
                    break;

                default:
                    // Show status + available actions so they're never stranded
                    await SendStatus(request.VkUserId, ticket, ct);
                    break;
            }
        }

        private async Task SendStatus(string vkUserId, Ticket ticket, CancellationToken ct)
        {
            var room = await _db.Rooms.AsNoTracking().Include(r => r.Layout)
                .FirstOrDefaultAsync(r => r.Id == ticket.RoomId, ct);

            var assetName    = room?.Layout.FindAsset(ticket.PlacedAssetId)?.AssetName ?? "Unknown";
            var statusText   = VkKeyboard.HumanStatus(ticket.Status);
            var keyboard     = VkKeyboard.ForStatus(ticket.Status);

            await _vkBot.SendMessageAsync(vkUserId,
                $"📋 Ticket Status\n" +
                $"Asset: {assetName}\n" +
                $"Room:  {room?.Name ?? "Unknown"}\n" +
                $"Status: {statusText}",
                keyboard, ct);
        }

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
                var keyboard = VkKeyboard.ForStatus(ticket.Status);
                await _vkBot.SendMessageAsync(request.VkUserId,
                    "⚠️ Input was empty. Please try again.", keyboard, ct);
                return;
            }

            try
            {
                (Action<Ticket> action, string successMsg) = pendingCmd switch
                {
                    "needs_parts" => ((Action<Ticket>)(t => t.ReportNeedsParts(maintainerId, input)), "🔧 Reported: needs parts."),
                    "irreparable" => ((Action<Ticket>)(t => t.ReportIrreparable(maintainerId, input)), "❌ Reported as irreparable."),
                    "inspection_repairable" => ((Action<Ticket>)(t => t.SubmitInspectionResult(maintainerId, true, input)), "🔍 Inspection submitted: repairable."),
                    "inspection_irreparable" => ((Action<Ticket>)(t => t.SubmitInspectionResult(maintainerId, false, input)), "🔍 Inspection submitted: irreparable."),
                    _ => throw new InvalidOperationException("Unknown pending command."),  // ensures exhaustiveness
                };

                await ExecuteTransition(request.VkUserId, ticket, maintainerId, action, successMsg, ct);
            }
            catch (DomainException ex)
            {
                var keyboard = VkKeyboard.ForStatus(ticket.Status);
                await _vkBot.SendMessageAsync(request.VkUserId, $"❌ {ex.Message}", keyboard, ct);
            }
        }

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

                var newKeyboard  = VkKeyboard.ForStatus(ticket.Status);
                var statusText   = VkKeyboard.HumanStatus(ticket.Status);
                await _vkBot.SendMessageAsync(vkUserId,
                    $"{successMessage}\n\nCurrent status: {statusText}",
                    newKeyboard, ct);
            }
            catch (DomainException ex)
            {
                var keyboard = VkKeyboard.ForStatus(ticket.Status);
                await _vkBot.SendMessageAsync(vkUserId, $"❌ {ex.Message}", keyboard, ct);
            }
        }

        private static string? ParseCommand(string? payload)
        {
            if (string.IsNullOrEmpty(payload)) return null;
            try
            {
                var doc = JsonDocument.Parse(payload);
                return doc.RootElement.TryGetProperty("cmd", out var cmd) ? cmd.GetString() : null;
            }
            catch { return null; }
        }

        private static bool HasButtonPayload(string? payload)
            => !string.IsNullOrEmpty(payload) && ParseCommand(payload) != null;
    }
}
