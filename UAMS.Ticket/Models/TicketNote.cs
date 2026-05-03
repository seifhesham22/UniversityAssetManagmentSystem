using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Ticket.Models
{
    public sealed class TicketNote
    {
        public Guid Id { get; private set; }
        public Guid TicketId { get; private set; }
        public Guid AuthorId { get; private set; }
        public string Content { get; private set; } = null!;
        public DateTime CreatedAtUtc { get; private set; }

        private TicketNote() { }

        internal TicketNote(Guid ticketId, Guid authorId, string content)
        {
            if (ticketId == Guid.Empty) throw new ArgumentException(nameof(ticketId));
            if (authorId == Guid.Empty) throw new ArgumentException(nameof(authorId));

            Id = Guid.NewGuid();
            TicketId = ticketId;
            AuthorId = authorId;
            Content = content;
            CreatedAtUtc = DateTime.UtcNow;
        }
    }
}
