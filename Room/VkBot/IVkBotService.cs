namespace UAMS.Room.VkBot
{
    public interface IVkBotService
    {
        Task<bool> SendMessageAsync(
            string vkUserId,
            string message,
            string? keyboardJson = null,
            CancellationToken ct = default);

        /// <summary>
        /// Acknowledge a callback button press. Must be called within 25 s or VK shows an error.
        /// </summary>
        Task AnswerCallbackAsync(
            string eventId,
            string userId,
            string peerId,
            string snackbarText = "⏳ Updating...",
            CancellationToken ct = default);
    }
}
