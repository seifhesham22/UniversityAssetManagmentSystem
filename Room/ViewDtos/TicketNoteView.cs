namespace UAMS.Room.ViewDtos
{
    public sealed record TicketNoteView(
        Guid Id, Guid AuthorId, string AuthorName,
        string Content, DateTime CreatedAtUtc);
}
