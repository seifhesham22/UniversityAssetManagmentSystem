using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using UAMS.Room.Features.TicketFeatures.HandleVkMessage;
using UAMS.Room.VkBot;

namespace UAMS.API.Controllers
{
    [ApiController]
    [Route("api/vk")]
    [AllowAnonymous]
    public sealed class VkWebhookController(
        IMediator _mediator,
        IVkBotService _vkBot,
        IConfiguration _cfg) : ControllerBase
    {
        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook([FromBody] JsonElement body, CancellationToken ct)
        {
            var type = body.GetProperty("type").GetString();

            // VK confirmation handshake
            if (type == "confirmation")
                return Ok(_cfg["Vk:ConfirmationString"]);

            // ── Regular text message (used for multi-step text input) ──────────
            if (type == "message_new")
            {
                try
                {
                    var message = body.GetProperty("object").GetProperty("message");
                    var fromId  = message.GetProperty("from_id").GetInt64().ToString();
                    var text    = message.TryGetProperty("text",    out var t) ? t.GetString() ?? "" : "";
                    // In message_new, payload is a string containing JSON
                    var payload = message.TryGetProperty("payload", out var p) ? p.GetString() : null;

                    if (!string.IsNullOrWhiteSpace(text) || !string.IsNullOrWhiteSpace(payload))
                        await _mediator.Send(new HandleVkMessageCommand(fromId, text, payload), ct);
                }
                catch { }
            }

            // ── Callback button press ─────────────────────────────────────────
            else if (type == "message_event")
            {
                try
                {
                    var obj     = body.GetProperty("object");
                    var userId  = obj.GetProperty("user_id").GetInt64().ToString();
                    var peerId  = obj.GetProperty("peer_id").GetInt64().ToString();
                    var eventId = obj.GetProperty("event_id").GetString()!;

                    // In message_event, payload is already a JSON object — use GetRawText()
                    var payload = obj.TryGetProperty("payload", out var p) ? p.GetRawText() : null;

                    // Acknowledge the button press immediately (VK requires this within 25 s)
                    await _vkBot.AnswerCallbackAsync(eventId, userId, peerId, "⏳ Updating...", ct);

                    if (!string.IsNullOrWhiteSpace(payload))
                        await _mediator.Send(new HandleVkMessageCommand(userId, "", payload), ct);
                }
                catch { }
            }

            return Ok("ok");
        }
    }
}
