using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace UAMS.Room.VkBot
{
    public sealed class VkBotService(IHttpClientFactory _httpFactory, IConfiguration _cfg) : IVkBotService
    {
        private static readonly Uri SendUri   = new("https://api.vk.com/method/messages.send");
        private static readonly Uri AnswerUri = new("https://api.vk.com/method/messages.sendMessageEventAnswer");

        public async Task<bool> SendMessageAsync(
            string vkUserId,
            string message,
            string? keyboardJson = null,
            CancellationToken ct = default)
        {
            try
            {
                var token   = _cfg["Vk:AccessToken"];
                var version = _cfg["Vk:ApiVersion"];

                var pairs = new List<KeyValuePair<string, string>>
                {
                    new("user_id",      vkUserId),
                    new("message",      message),
                    new("random_id",    Random.Shared.Next(1, int.MaxValue).ToString()),
                    new("access_token", token!),
                    new("v",            version!),
                };

                if (keyboardJson is not null)
                    pairs.Add(new("keyboard", keyboardJson));

                var http     = _httpFactory.CreateClient("vk");
                var response = await http.PostAsync(SendUri, new FormUrlEncodedContent(pairs), ct);

                if (!response.IsSuccessStatusCode) return false;

                var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
                return json.TryGetProperty("response", out _);
            }
            catch
            {
                return false;
            }
        }

        public async Task AnswerCallbackAsync(
            string eventId,
            string userId,
            string peerId,
            string snackbarText = "⏳ Updating...",
            CancellationToken ct = default)
        {
            try
            {
                var token   = _cfg["Vk:AccessToken"];
                var version = _cfg["Vk:ApiVersion"];

                var eventData = JsonSerializer.Serialize(new { type = "show_snackbar", text = snackbarText });

                var pairs = new[]
                {
                    new KeyValuePair<string, string>("event_id",      eventId),
                    new KeyValuePair<string, string>("user_id",       userId),
                    new KeyValuePair<string, string>("peer_id",       peerId),
                    new KeyValuePair<string, string>("event_data",    eventData),
                    new KeyValuePair<string, string>("access_token",  token!),
                    new KeyValuePair<string, string>("v",             version!),
                };

                var http = _httpFactory.CreateClient("vk");
                await http.PostAsync(AnswerUri, new FormUrlEncodedContent(pairs), ct);
            }
            catch { /* best-effort */ }
        }
    }
}
