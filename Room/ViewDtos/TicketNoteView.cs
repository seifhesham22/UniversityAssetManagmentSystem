namespace UAMS.Room.ViewDtos
{
    public sealed record TicketNoteView(
        Guid Id, Guid AuthorId, string AuthorName, string AuthorRole,
        string Content, DateTime CreatedAtUtc);
}
