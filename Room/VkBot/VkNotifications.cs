namespace UAMS.Room.VkBot
{
    internal static class VkNotifications
    {
        public static string TicketAssigned(
            Guid ticketId,
            string assetName,
            string roomName,
            string buildingName,
            string? buildingAddress,
            string facultyName,
            string? assetManagerName)
        {
            var lines = new System.Text.StringBuilder();
            lines.AppendLine("🔧 New ticket assigned to you!");
            lines.AppendLine();
            lines.AppendLine($"🏷  Asset:    {assetName}");
            lines.AppendLine($"🚪 Room:     {roomName}");
            lines.AppendLine($"🏢 Building: {buildingName}");
            if (!string.IsNullOrEmpty(buildingAddress))
                lines.AppendLine($"📍 Address:  {buildingAddress}");
            lines.AppendLine($"🎓 Faculty:  {facultyName}");
            if (!string.IsNullOrEmpty(assetManagerName))
                lines.AppendLine($"👤 Manager:  {assetManagerName}");
            lines.AppendLine($"🔖 Ticket:   #{ticketId.ToString()[..8].ToUpper()}");
            lines.AppendLine();
            lines.Append("Use the buttons below to update the ticket status.");
            return lines.ToString();
        }

        public static string Reassigned(
            Guid ticketId,
            string assetName,
            string roomName,
            string buildingName,
            string? buildingAddress,
            string facultyName,
            string? assetManagerName,
            IEnumerable<(string Author, string Content, DateTime At)> notes)
        {
            var lines = new System.Text.StringBuilder();
            lines.AppendLine("🔄 Ticket reassigned to you!");
            lines.AppendLine();
            lines.AppendLine($"🏷  Asset:    {assetName}");
            lines.AppendLine($"🚪 Room:     {roomName}");
            lines.AppendLine($"🏢 Building: {buildingName}");
            if (!string.IsNullOrEmpty(buildingAddress))
                lines.AppendLine($"📍 Address:  {buildingAddress}");
            lines.AppendLine($"🎓 Faculty:  {facultyName}");
            if (!string.IsNullOrEmpty(assetManagerName))
                lines.AppendLine($"👤 Manager:  {assetManagerName}");
            lines.AppendLine($"🔖 Ticket:   #{ticketId.ToString()[..8].ToUpper()}");

            var noteList = notes.ToList();
            if (noteList.Count > 0)
            {
                lines.AppendLine();
                lines.AppendLine("📝 History:");
                foreach (var n in noteList.TakeLast(5))
                    lines.AppendLine($"  [{n.At:dd.MM HH:mm}] {n.Author}: {n.Content}");
            }

            lines.AppendLine();
            lines.Append("Use the buttons below to update the ticket status.");
            return lines.ToString();
        }

        public static string PreviousMaintainerUnassigned(
            Guid ticketId,
            string assetName,
            IEnumerable<(string Author, string Content, DateTime At)> notes)
        {
            var lines = new System.Text.StringBuilder();
            lines.AppendLine($"ℹ️ Ticket #{ticketId.ToString()[..8].ToUpper()} ({assetName}) has been reassigned to another maintainer.");
            lines.AppendLine();

            var noteList = notes.ToList();
            if (noteList.Count > 0)
            {
                lines.AppendLine("📝 Ticket history for your records:");
                foreach (var n in noteList)
                    lines.AppendLine($"  [{n.At:dd.MM HH:mm}] {n.Author}: {n.Content}");
            }

            return lines.ToString();
        }
    }
}
